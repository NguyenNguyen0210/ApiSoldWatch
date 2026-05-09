using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ShopNN.Entities;
using ShopNN.Repositories.Interface;

namespace ShopNN.Repositories.Implement
{
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        private readonly ApplicationDbContext _context;
        public OrderRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
        public async override Task<IEnumerable<Order>> GetAllAsync()
        {
            return await _context.Orders
                   .Include(o => o.Items)
                       .ThenInclude(i => i.Product)
                   .OrderByDescending(o => o.CreatedAt)
                   .AsNoTracking()
                   .ToListAsync();
        }
        public override async Task<Order?> GetByIdAsync(Guid id) =>
              await _context.Orders
                  .Include(o => o.Items)
                      .ThenInclude(i => i.Product)
                  .FirstOrDefaultAsync(o => o.Id == id);
        public async Task<List<Order>> GetByUserIdAsync(Guid userId) =>
       await _context.Orders
           .Include(o => o.Items)
               .ThenInclude(i => i.Product)
           .Where(o => o.UserId == userId)
           .OrderByDescending(o => o.CreatedAt)
           .AsNoTracking()
           .ToListAsync();

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }
    }
}
