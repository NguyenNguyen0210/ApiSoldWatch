using ShopNN.Entities;
using ShopNN.DTOs;
namespace ShopNN.Services.Interface
{
    public interface IAuthService
    {
        Task<TokenResponseDTO> GenerateTokenAsync(ApplicationUser User);
        Task<TokenResponseDTO> RefreshToken(string token);
        Task<string> GenerateAccessTokenAsync(ApplicationUser user);
        Task SaveRefreshTokenAsync(string refreshToken, ApplicationUser user);
        Task Revoke(string refreshToken);

    }
}
