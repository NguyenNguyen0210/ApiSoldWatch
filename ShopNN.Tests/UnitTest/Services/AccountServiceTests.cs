using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using ShopNN.DTOs;
using ShopNN.Entities;
using ShopNN.Services.Implement;
using ShopNN.Services.Interface;
using ShopNN.Shared.Exeptions;

namespace ShopNN.Tests.Services;

public class AccountServiceTests
{
    private readonly Mock<IAuthService> _auth = new();
    private readonly Mock<UserManager<ApplicationUser>> _um = UserManagerStub();
    private readonly Mock<RoleManager<ApplicationRole>> _rm = RoleManagerStub();
    private readonly AccountService _sut;

    public AccountServiceTests()
    {
        _sut = new AccountService(_auth.Object, _um.Object, _rm.Object);
    }

    private static Mock<UserManager<ApplicationUser>> UserManagerStub()
    {
        var store = Mock.Of<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(
            store,
            Options.Create(new IdentityOptions()),
            new PasswordHasher<ApplicationUser>(),
            Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            null!,
            Mock.Of<ILogger<UserManager<ApplicationUser>>>());
    }

    private static Mock<RoleManager<ApplicationRole>> RoleManagerStub()
    {
        var store = Mock.Of<IRoleStore<ApplicationRole>>();
        return new Mock<RoleManager<ApplicationRole>>(
            store,
            Array.Empty<IRoleValidator<ApplicationRole>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            Mock.Of<ILogger<RoleManager<ApplicationRole>>>());
    }

    private static ApplicationUser MakeUser(string username = "testuser") => new()
    {
        Id       = Guid.NewGuid(),
        UserName = username,
        Email    = $"{username}@mail.com"
    };

    private static TokenResponseDTO MakeTokens() => new()
    {
        AccessToken  = "access-token",
        RefreshToken = "refresh-token"
    };

    #region SignIn
    [Fact]
    public async Task SignIn_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        _um.Setup(m => m.FindByNameAsync("nguyen"))
           .ReturnsAsync((ApplicationUser?)null);

        var act = async () => await _sut.SignIn(new SignInDTO { Username = "nguyen", Password = "Top1zuka@" });

        await act.Should().ThrowAsync<NotFoundException>()
                 .WithMessage("User not found");

        _auth.Verify(a => a.GenerateTokenAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task SignIn_WhenPasswordIncorrect_ShouldThrowBadRequestException()
    {
        var user = MakeUser();
        _um.Setup(m => m.FindByNameAsync(user.UserName!)).ReturnsAsync(user);
        _um.Setup(m => m.CheckPasswordAsync(user, "wrong")).ReturnsAsync(false);

        var act = async () => await _sut.SignIn(new SignInDTO { Username = user.UserName!, Password = "wrong" });

        await act.Should().ThrowAsync<BadRequestException>()
                 .WithMessage("Password Incorrect");

        _auth.Verify(a => a.GenerateTokenAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task SignIn_WhenCredentialsValid_ShouldReturnTokens()
    {
        var user   = MakeUser();
        var tokens = MakeTokens();

        _um.Setup(m => m.FindByNameAsync(user.UserName!)).ReturnsAsync(user);
        _um.Setup(m => m.CheckPasswordAsync(user, "correct")).ReturnsAsync(true);
        _auth.Setup(a => a.GenerateTokenAsync(user)).ReturnsAsync(tokens);

        var result = await _sut.SignIn(new SignInDTO { Username = user.UserName!, Password = "correct" });

        result.AccessToken.Should().Be(tokens.AccessToken);
        result.RefreshToken.Should().Be(tokens.RefreshToken);
        _auth.Verify(a => a.GenerateTokenAsync(user), Times.Once);
    }

    [Fact]
    public async Task SignIn_WhenTokenGenerationFails_ShouldPropagate()
    {
        var user = MakeUser();
        _um.Setup(m => m.FindByNameAsync(user.UserName!)).ReturnsAsync(user);
        _um.Setup(m => m.CheckPasswordAsync(user, "ok")).ReturnsAsync(true);
        _auth.Setup(a => a.GenerateTokenAsync(user))
             .ThrowsAsync(new Exception("Token service down"));

        var act = async () => await _sut.SignIn(new SignInDTO { Username = user.UserName!, Password = "ok" });

        await act.Should().ThrowAsync<Exception>()
                 .WithMessage("Token service down");
    }
    #endregion

    #region SignUp
    [Fact]
    public async Task SignUp_WhenCreateFails_ShouldReturnFailedResult()
    {
        _um.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
           .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Email taken" }));

        var result = await _sut.SignUp(new SignUpDTO { Username = "u", Password = "P@ss123!", Email = "u@mail.com" });

        result.Succeeded.Should().BeFalse();

        // Role không được gán khi create fail
        _um.Verify(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SignUp_WhenSucceededAndRoleExists_ShouldAddRoleWithoutCreating()
    {
        _um.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
           .ReturnsAsync(IdentityResult.Success);
        _um.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"))
           .ReturnsAsync(IdentityResult.Success);
        _rm.Setup(r => r.RoleExistsAsync("User")).ReturnsAsync(true);

        var result = await _sut.SignUp(new SignUpDTO { Username = "u", Password = "P@ss123!", Email = "u@mail.com" });

        result.Succeeded.Should().BeTrue();

        // Role đã tồn tại → không cần tạo mới
        _rm.Verify(r => r.CreateAsync(It.IsAny<ApplicationRole>()), Times.Never);
        _um.Verify(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"), Times.Once);
    }

    [Fact]
    public async Task SignUp_WhenSucceededAndRoleNotExists_ShouldCreateRoleThenAdd()
    {
        _um.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
           .ReturnsAsync(IdentityResult.Success);
        _um.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"))
           .ReturnsAsync(IdentityResult.Success);
        _rm.Setup(r => r.RoleExistsAsync("User")).ReturnsAsync(false);
        _rm.Setup(r => r.CreateAsync(It.IsAny<ApplicationRole>()))
           .ReturnsAsync(IdentityResult.Success);

        var result = await _sut.SignUp(new SignUpDTO { Username = "u", Password = "P@ss123!", Email = "u@mail.com" });

        result.Succeeded.Should().BeTrue();

        _rm.Verify(r => r.CreateAsync(It.IsAny<ApplicationRole>()), Times.Once);
        _um.Verify(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"), Times.Once);
    }
    #endregion


    #region RefreshToken
    [Fact]
    public async Task RefreshToken_WhenCalled_ShouldDelegateToAuthService()
    {
        var tokens = MakeTokens();
        _auth.Setup(a => a.RefreshToken("old-token")).ReturnsAsync(tokens);

        var result = await _sut.RefreshToken(new RefreshTokenRequestDTO { Token = "old-token" });

        result.AccessToken.Should().Be(tokens.AccessToken);
        result.RefreshToken.Should().Be(tokens.RefreshToken);
        _auth.Verify(a => a.RefreshToken("old-token"), Times.Once);
    }

    [Fact]
    public async Task RefreshToken_WhenTokenInvalid_ShouldPropagate()
    {
        _auth.Setup(a => a.RefreshToken(It.IsAny<string>()))
             .ThrowsAsync(new SecurityTokenException("Invalid or expired refresh token."));

        var act = async () => await _sut.RefreshToken(new RefreshTokenRequestDTO { Token = "bad" });

        await act.Should().ThrowAsync<SecurityTokenException>();
    }
    #endregion


    #region SignOut
    [Fact]
    public async Task SignOut_WhenCalled_ShouldRevokeTokenViaAuthService()
    {
        await _sut.SignOut(new RefreshTokenRequestDTO { Token = "token-to-revoke" });

        _auth.Verify(a => a.Revoke("token-to-revoke"), Times.Once);
    }
    #endregion

    #region FindByUserId
    [Fact]
    public async Task FindByUserId_WhenNotFound_ShouldThrowNotFoundException()
    {
        _um.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
           .ReturnsAsync((ApplicationUser?)null);

        var act = async () => await _sut.FindByUserId(Guid.NewGuid().ToString());

        await act.Should().ThrowAsync<NotFoundException>()
                 .WithMessage("User not found");
    }

    [Fact]
    public async Task FindByUserId_WhenFound_ShouldReturnCorrectUser()
    {
        var user = MakeUser();
        _um.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);

        var result = await _sut.FindByUserId(user.Id.ToString());

        result.Id.Should().Be(user.Id);
        result.UserName.Should().Be(user.UserName);
        _um.Verify(m => m.FindByIdAsync(user.Id.ToString()), Times.Once);
    }
}
#endregion

