using Microsoft.EntityFrameworkCore;
using ShopNN.Entities;
using ShopNN.Repositories.Implement;
using ShopNN.Repositories.Interface;

namespace ShopNN.Repositories.Implement
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _context;
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        public async override Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products.AsNoTracking().Include(p => p.Category).ToListAsync();
        }
        public async override Task<Product?> GetByIdAsync(Guid id)
        {
            return await _context.Products.AsNoTracking().Include(p=>p.Category).FirstOrDefaultAsync(p=>p.Id == id);
        }
    }

}
