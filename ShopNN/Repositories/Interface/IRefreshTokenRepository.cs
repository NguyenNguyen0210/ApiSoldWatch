using ShopNN.Entities;

namespace ShopNN.Repositories.Interface
{

    public interface IRefreshTokenRepository
    {
        Task<RefreshToken> AddAsync(RefreshToken data);
        Task<RefreshToken?> GetByTokenAsync(string token);        
        Task<RefreshToken?> GetActiveByTokenAsync(string token);   
        Task SaveChangesAsync();
    }
}
