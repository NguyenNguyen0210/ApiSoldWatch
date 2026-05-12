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
    // ── Class-level setup ────────────────────────────────
    private readonly Mock<IRefreshTokenRepository> _repo = new();
    private readonly Mock<UserManager<ApplicationUser>> _um = UserManagerStub();
    private readonly IConfiguration _config = Mock.Of<IConfiguration>(
        c => c["Jwt:Key"] == "8c478937056e6633074fae54806f94ba8583ad0ce9504a83b712ad346c6c279e");
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _sut = new AuthService(_um.Object, _repo.Object, _config);
    }

    private static Mock<UserManager<ApplicationUser>> UserManagerStub()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        store.Setup(x => x.GetUserIdAsync(It.IsAny<ApplicationUser>(), default))
             .ReturnsAsync("id");
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

    private static ApplicationUser MakeUser() => new()
    {
        Id       = Guid.NewGuid(),
        UserName = "testuser",
        Email    = "test@example.com"
    };

    private static RefreshToken MakeRefreshToken(
        string token     = "valid-token",
        bool isRevoked   = false,
        int daysFromNow  = 7,
        ApplicationUser? user = null)
    {
        var u = user ?? MakeUser();
        return new RefreshToken
        {
            Token      = token,
            UserId     = u.Id,
            User       = u,
            IsRevoked  = isRevoked,
            ExpiryDate = DateTime.UtcNow.AddDays(daysFromNow),
            CreatedAt  = DateTime.UtcNow
        };
    }
    #region GenerateAccessTokenAsync
    [Fact]
    public async Task GenerateAccessTokenAsync_WhenJwtKeyMissing_ShouldThrow()
    {
        var config = Mock.Of<IConfiguration>(c => c["Jwt:Key"] == null);
        var sut    = new AuthService(_um.Object, _repo.Object, config);
        var user   = MakeUser();

        _um.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(Array.Empty<string>());

        var act = async () => await sut.GenerateAccessTokenAsync(user);

        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("*JWT Key*");
    }

    [Fact]
    public async Task GenerateAccessTokenAsync_WhenValid_ShouldReturnNonEmptyToken()
    {
        var user = MakeUser();
        _um.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new[] { "User" });

        var result = await _sut.GenerateAccessTokenAsync(user);

        result.Should().NotBeNullOrWhiteSpace();
        result.Split('.').Should().HaveCount(3);
    }

    [Fact]
    public async Task GenerateAccessTokenAsync_WhenUserHasRoles_ShouldIncludeRolesInToken()
    {
        var user = MakeUser();

        _um.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new[] { "User", "Admin" });

        var result = await _sut.GenerateAccessTokenAsync(user);

        result.Should().NotBeNullOrWhiteSpace();
        _um.Verify(m => m.GetRolesAsync(user), Times.Once);
    }

    [Fact]
    public async Task GenerateTokenAsync_WhenValid_ShouldReturnBothTokens()
    {
        var user = MakeUser();
        _um.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new[] { "User" });
        _repo.Setup(r => r.AddAsync(It.IsAny<RefreshToken>())).ReturnsAsync((RefreshToken r) => r);
        

        var result = await _sut.GenerateTokenAsync(user);

        result.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task GenerateTokenAsync_WhenValid_ShouldPersistRefreshToken()
    {
        var user = MakeUser();
        _um.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(Array.Empty<string>());

        RefreshToken? stored = null;
        _repo.Setup(r => r.AddAsync(It.IsAny<RefreshToken>()))
             .Callback<RefreshToken>(t => stored = t).ReturnsAsync((ShopNN.Entities.RefreshToken r) => r);
        

        var result = await _sut.GenerateTokenAsync(user);

        stored.Should().NotBeNull();
        stored!.Token.Should().Be(result.RefreshToken);
        stored.UserId.Should().Be(user.Id);
        stored.IsRevoked.Should().BeFalse();
        stored.ExpiryDate.Should().BeAfter(DateTime.UtcNow);

        _repo.Verify(r => r.AddAsync(It.IsAny<RefreshToken>()), Times.Once);
        
    }
    #endregion

    #region Revoke
    [Fact]
    public async Task Revoke_WhenTokenNotFound_ShouldThrowSecurityTokenException()
    {
        _repo.Setup(r => r.GetByTokenAsync(It.IsAny<string>()))
             .ReturnsAsync((RefreshToken?)null);

        var act = async () => await _sut.Revoke("nonexistent");

        await act.Should().ThrowAsync<SecurityTokenException>()
                 .WithMessage("*not found*");

        _repo.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Revoke_WhenTokenExists_ShouldMarkAsRevoked()
    {
        var refreshToken = MakeRefreshToken(isRevoked: false);
        _repo.Setup(r => r.GetByTokenAsync(refreshToken.Token))
             .ReturnsAsync(refreshToken);
        _repo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        await _sut.Revoke(refreshToken.Token);

        refreshToken.IsRevoked.Should().BeTrue();
        _repo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
    #endregion
    #region RefreshToken
    [Fact]
    public async Task RefreshToken_WhenTokenNotFound_ShouldThrow()
    {
        _repo.Setup(r => r.GetActiveByTokenAsync(It.IsAny<string>()))
             .ReturnsAsync((RefreshToken?)null);

        var act = async () => await _sut.RefreshToken("bad-token");

        await act.Should().ThrowAsync<SecurityTokenException>()
                 .WithMessage("Invalid or expired refresh token.");
    }

    [Fact]
    public async Task RefreshToken_WhenTokenRevoked_ShouldThrow()
    {
        _repo.Setup(r => r.GetActiveByTokenAsync("revoked"))
             .ReturnsAsync((RefreshToken?)null);

        var act = async () => await _sut.RefreshToken("revoked");

        await act.Should().ThrowAsync<SecurityTokenException>();
    }

    [Fact]
    public async Task RefreshToken_WhenValid_ShouldRevokeOldAndReturnNewTokens()
    {
        var user         = MakeUser();
        var refreshToken = MakeRefreshToken(token: "old-token", user: user);

        _repo.Setup(r => r.GetActiveByTokenAsync("old-token")).ReturnsAsync(refreshToken);
        _repo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        _repo.Setup(r => r.AddAsync(It.IsAny<RefreshToken>())).ReturnsAsync((ShopNN.Entities.RefreshToken r) => r);
        _um.Setup(m => m.GetRolesAsync(It.IsAny<ApplicationUser>()))
           .ReturnsAsync(Array.Empty<string>());

        var result = await _sut.RefreshToken("old-token");

        refreshToken.IsRevoked.Should().BeTrue();

        result.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();

        result.RefreshToken.Should().NotBe("old-token");

        _repo.Verify(r => r.AddAsync(It.IsAny<RefreshToken>()), Times.Once);
    }
    #endregion
}



