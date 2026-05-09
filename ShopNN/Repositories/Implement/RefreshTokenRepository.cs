using Microsoft.EntityFrameworkCore;
using ShopNN.Entities;
using ShopNN.Repositories.Interface;

namespace ShopNN.Repositories.Implement
{
    public class RefreshTokenRepository:GenericRepository<RefreshToken>,IRefreshTokenRepository
    {
        private readonly ApplicationDbContext _context;

        public RefreshTokenRepository(ApplicationDbContext context):base(context) 
        {
            _context = context;
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token) =>
            await _context.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == token);

        public async Task<RefreshToken?> GetActiveByTokenAsync(string token) =>
            await _context.RefreshTokens
                .Include(x => x.User)
                .FirstOrDefaultAsync(x =>
                    x.Token == token &&
                    !x.IsRevoked &&
                    x.ExpiryDate > DateTime.UtcNow);

        public async Task SaveChangesAsync() =>
            await _context.SaveChangesAsync();
    }
}
