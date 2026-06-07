using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using ShopNN.DTOs.Account;
using ShopNN.DTOs.Product;
using ShopNN.DTOs.Category;
using ShopNN.DTOs.Cart;
using ShopNN.DTOs.Order;
using ShopNN.Entities;
using ShopNN.Services.Implement;
using ShopNN.Shared.Enums;
using ShopNN.Services.Interface;
using ShopNN.Shared.Exceptions;

namespace ShopNN.Tests.Services;

public class AccountServiceTests
{
    private readonly Mock<IAuthService> _auth = new();
    private readonly Mock<UserManager<ApplicationUser>> _um = UserManagerStub();
    private readonly Mock<RoleManager<ApplicationRole>> _rm = RoleManagerStub();
    private readonly Mock<IEmailService> _email = new();
    private readonly Mock<ILogger<AccountService>> _logger = new();
    private readonly AccountService _sut;

    public AccountServiceTests()
    {
        _sut = new AccountService(_auth.Object, _um.Object, _rm.Object, _email.Object, _logger.Object);
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
        _um.Setup(m => m.CheckPasswordAsync(user, "abc")).ReturnsAsync(false);

        var act = async () => await _sut.SignIn(new SignInDTO { Username = user.UserName!, Password = "abc" });

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
        _um.Setup(m => m.CheckPasswordAsync(user, "abc123")).ReturnsAsync(true);
        _auth.Setup(a => a.GenerateTokenAsync(user)).ReturnsAsync(tokens);

        var result = await _sut.SignIn(new SignInDTO { Username = user.UserName!, Password = "abc123" });

        result.AccessToken.Should().Be(tokens.AccessToken);
        result.RefreshToken.Should().Be(tokens.RefreshToken);
        _auth.Verify(a => a.GenerateTokenAsync(user), Times.Once);
    }

    #endregion

    #region SignUp
    [Fact]
    public async Task SignUp_WhenCreateFails_ShouldThrowBadRequestException()
    {
        _um.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
           .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Email taken" }));

        var act = async () => await _sut.SignUp(new SignUpDTO { Username = "u", Password = "P@ss123!", Email = "u@mail.com" });

        await act.Should().ThrowAsync<BadRequestException>()
                 .WithMessage("Đăng ký thất bại: Email taken");

        _um.Verify(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        _email.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SignUp_WhenSucceededAndRoleExists_ShouldAddRoleAndSendEmail()
    {
        _um.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
           .ReturnsAsync(IdentityResult.Success);
        _um.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), RoleNames.User))
           .ReturnsAsync(IdentityResult.Success);
        _rm.Setup(r => r.RoleExistsAsync(RoleNames.User)).ReturnsAsync(true);

        var result = await _sut.SignUp(new SignUpDTO { Username = "u", Password = "P@ss123!", Email = "u@mail.com" });

        result.UserName.Should().Be("u");
        result.Email.Should().Be("u@mail.com");

        _rm.Verify(r => r.CreateAsync(It.IsAny<ApplicationRole>()), Times.Never);
        _um.Verify(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), RoleNames.User), Times.Once);
        _email.Verify(e => e.SendEmailAsync("u@mail.com", It.IsAny<string>(), It.IsAny<string>(), "u"), Times.Once);
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

        var act = async () => await _sut.RefreshToken(new RefreshTokenRequestDTO { Token = "abcd" });

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


