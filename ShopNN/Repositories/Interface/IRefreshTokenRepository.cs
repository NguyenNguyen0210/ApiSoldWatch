using ShopNN.Entities;

namespace ShopNN.Repositories.Interface
{

    // Repositories/Interface/IRefreshTokenRepository.cs
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken> AddAsync(RefreshToken data);
        Task<RefreshToken?> GetByTokenAsync(string token);         // dùng cho Revoke + RefreshToken
        Task<RefreshToken?> GetActiveByTokenAsync(string token);   // include User, chưa revoked, chưa expired
        Task SaveChangesAsync();
    }
}
