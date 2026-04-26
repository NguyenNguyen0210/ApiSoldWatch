using ShopNN.Entities;

namespace ShopNN.Services.Interface
{
    public interface IAuthService
    {
        Task<string> GenerateRefreshTokenAsync();
        Task<string> GenerateAccessTokenAsync(ApplicationUser user);
        Task SaveRefreshTokenAsync(string refreshToken, ApplicationUser user);
        Task Revoke(string refreshToken);

    }
}
