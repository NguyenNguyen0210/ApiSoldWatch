using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using ShopNN.Entities;
using ShopNN.Repositories.Interface;
using ShopNN.Services.Implement;

namespace ShopNN.Tests.Services;

public class AuthServiceTests
{
    private static Mock<UserManager<ApplicationUser>> UserManagerStub()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        store.Setup(x => x.GetUserIdAsync(It.IsAny<ApplicationUser>(), default)).Returns(Task.FromResult("id"));
        return new Mock<UserManager<ApplicationUser>>(
            store.Object,
            Options.Create(new IdentityOptions()),
            new PasswordHasher<ApplicationUser>(),
            Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            null!,
            Mock.Of<ILogger<UserManager<ApplicationUser>>>());
    }

    [Fact]
    public async Task GenerateAccessTokenAsync_WhenJwtKeyMissing_ShouldThrow()
    {
        var config = Mock.Of<IConfiguration>(c => c["Jwt:Key"] == null);
        var userManager = UserManagerStub();
        var repo = new Mock<IRefreshTokenRepository>();
        userManager.Setup(m => m.GetRolesAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(Array.Empty<string>());

        var sut = new AuthService(userManager.Object, repo.Object, config);

        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = "tester", Email = "t@example.com" };
        var act = async () => await sut.GenerateAccessTokenAsync(user);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task GenerateTokenAsync_ShouldReturnAccessAndPersistRefreshToken()
    {
        var jwtKey = "DevelopmentSigningKey_Over32Characters_XXX!!!";
        var config = Mock.Of<IConfiguration>(c => c["Jwt:Key"] == jwtKey);

        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = "u1", Email = "u@c.com" };

        var userManager = UserManagerStub();
        userManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new[] { "User" });

        RefreshToken? stored = null;
        var repo = new Mock<IRefreshTokenRepository>();
        repo.Setup(r => r.AddAsync(It.IsAny<RefreshToken>()))
            .Callback<RefreshToken>(t => stored = t)
            .ReturnsAsync((RefreshToken e) => e);
        repo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var sut = new AuthService(userManager.Object, repo.Object, config);
        var result = await sut.GenerateTokenAsync(user);

        result.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
        stored.Should().NotBeNull();
        stored!.Token.Should().Be(result.RefreshToken);
        repo.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Revoke_WhenTokenMissing_ShouldThrow()
    {
        var jwtKey = "DevelopmentSigningKey_Over32Characters_XXX!!!";
        var config = Mock.Of<IConfiguration>(c => c["Jwt:Key"] == jwtKey);
        var userManager = UserManagerStub();
        var repo = new Mock<IRefreshTokenRepository>();
        repo.Setup(r => r.GetByTokenAsync(It.IsAny<string>())).ReturnsAsync((RefreshToken?)null);

        var sut = new AuthService(userManager.Object, repo.Object, config);

        var act = async () => await sut.Revoke("any");

        await act.Should().ThrowAsync<SecurityTokenException>();
    }
}
