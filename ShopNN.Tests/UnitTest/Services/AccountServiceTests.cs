using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using ShopNN.DTOs;
using ShopNN.Entities;
using ShopNN.Services.Implement;
using ShopNN.Services.Interface;
using ShopNN.Shared.Exeptions;

namespace ShopNN.Tests.Services;

public class AccountServiceTests
{
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

    [Fact]
    public async Task SignIn_WhenUserMissing_ShouldThrowNotFound()
    {
        var auth = new Mock<IAuthService>();
        var um = UserManagerStub();
        um.Setup(m => m.FindByNameAsync("noone")).ReturnsAsync((ApplicationUser?)null);

        var sut = new AccountService(auth.Object, um.Object, RoleManagerStub().Object);

        var act = async () => await sut.SignIn(new SignInDTO { Username = "noone", Password = "x" });

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("User not found");
        auth.Verify(a => a.GenerateTokenAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task SignIn_WhenPasswordWrong_ShouldThrowBadRequest()
    {
        var auth = new Mock<IAuthService>();
        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = "a" };
        var um = UserManagerStub();
        um.Setup(m => m.FindByNameAsync("a")).ReturnsAsync(user);
        um.Setup(m => m.CheckPasswordAsync(user, "bad")).ReturnsAsync(false);

        var sut = new AccountService(auth.Object, um.Object, RoleManagerStub().Object);

        var act = async () => await sut.SignIn(new SignInDTO { Username = "a", Password = "bad" });

        await act.Should().ThrowAsync<BadRequestException>().WithMessage("Password Incorrect");
    }

    [Fact]
    public async Task SignIn_WhenOk_ShouldReturnTokens()
    {
        var auth = new Mock<IAuthService>();
        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = "a" };
        var um = UserManagerStub();
        um.Setup(m => m.FindByNameAsync("a")).ReturnsAsync(user);
        um.Setup(m => m.CheckPasswordAsync(user, "ok")).ReturnsAsync(true);

        auth.Setup(a => a.GenerateTokenAsync(user))
            .ReturnsAsync(new TokenResponseDTO { AccessToken = "jwt", RefreshToken = "r1" });

        var sut = new AccountService(auth.Object, um.Object, RoleManagerStub().Object);

        var tokens = await sut.SignIn(new SignInDTO { Username = "a", Password = "ok" });

        tokens.AccessToken.Should().Be("jwt");
        tokens.RefreshToken.Should().Be("r1");
    }

    [Fact]
    public async Task SignUp_WhenCreateFails_ShouldReturnIdentityResultWithoutRoleWork()
    {
        var um = UserManagerStub();
        um.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "fail" }));

        var sut = new AccountService(Mock.Of<IAuthService>(), um.Object, RoleManagerStub().Object);

        var result = await sut.SignUp(new SignUpDTO { Username = "u", Password = "P@ss123!", Email = "u@mail.com" });

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task SignUp_WhenSucceededAndRoleExists_ShouldAddRole()
    {
        var auth = new Mock<IAuthService>();
        var um = UserManagerStub();
        um.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
        um.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User")).ReturnsAsync(IdentityResult.Success);
        var roles = RoleManagerStub();
        roles.Setup(r => r.RoleExistsAsync("User")).ReturnsAsync(true);

        var sut = new AccountService(auth.Object, um.Object, roles.Object);

        var result = await sut.SignUp(new SignUpDTO { Username = "u", Password = "P@ss123!", Email = "u@mail.com" });

        result.Succeeded.Should().BeTrue();
        um.Verify(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"), Times.Once);
    }

    [Fact]
    public async Task RefreshToken_ShouldForwardToAuth()
    {
        var auth = new Mock<IAuthService>();
        auth.Setup(a => a.RefreshToken("tok")).ReturnsAsync(new TokenResponseDTO { AccessToken = "na", RefreshToken = "nr" });
        var um = UserManagerStub();
        var sut = new AccountService(auth.Object, um.Object, RoleManagerStub().Object);

        var r = await sut.RefreshToken(new RefreshTokenRequestDTO { Token = "tok" });

        r.RefreshToken.Should().Be("nr");
        auth.Verify(a => a.RefreshToken("tok"), Times.Once);
    }

    [Fact]
    public async Task SignOut_ShouldRevokeViaAuth()
    {
        var auth = new Mock<IAuthService>();
        var um = UserManagerStub();
        var sut = new AccountService(auth.Object, um.Object, RoleManagerStub().Object);

        await sut.SignOut(new RefreshTokenRequestDTO { Token = "t" });

        auth.Verify(a => a.Revoke("t"), Times.Once);
    }

    [Fact]
    public async Task FindByUserId_WhenMissing_ShouldThrow()
    {
        var um = UserManagerStub();
        um.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

        var sut = new AccountService(Mock.Of<IAuthService>(), um.Object, RoleManagerStub().Object);

        var act = async () => await sut.FindByUserId(Guid.NewGuid().ToString());

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("User not found");
    }

    [Fact]
    public async Task FindByUserId_WhenFound_ShouldReturnUser()
    {
        var uid = Guid.NewGuid();
        var user = new ApplicationUser { Id = uid, UserName = "uu" };
        var um = UserManagerStub();
        um.Setup(m => m.FindByIdAsync(uid.ToString())).ReturnsAsync(user);

        var sut = new AccountService(Mock.Of<IAuthService>(), um.Object, RoleManagerStub().Object);

        var found = await sut.FindByUserId(uid.ToString());

        found.Id.Should().Be(uid);
    }
}
