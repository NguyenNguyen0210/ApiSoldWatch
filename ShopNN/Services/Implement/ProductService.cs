using Microsoft.EntityFrameworkCore;
using ShopNN.DTOs;
using ShopNN.Entities;
using ShopNN.Services.Interface;

namespace ShopNN.Services.Implement
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;
        public ProductService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Product> CreateAsync(ProductDTO dto)
        {
            Product product = new Product()
            {
                Name = dto.Name,
                Price = dto.Price,
                Stock = dto.Stock,
                Description = dto.Description,
                Id = Guid.NewGuid(),
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;


        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            Product product = await _context.Products.FirstOrDefaultAsync(x => x.Id ==id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;


        }

        public async Task<List<Product>> GetAllAsync()
        {
            List<Product> products = await _context.Products.ToListAsync();
            return products;
        }

        public async Task<Product?> GetByIdAsync(Guid id)
        {
            Product product = await _context.Products.FirstOrDefaultAsync(x => x.Id == id);
            if (product != null)
            {
                return product;
            }
            return null;
        }

        public async Task<Product> UpdateAsync(Guid id, ProductDTO dto)
        {
            Product product = await _context.Products.FirstOrDefaultAsync(x => x.Id == id);
            if (product != null) {
                product.Name = dto.Name;
                product.Description = dto.Description;
                product.Price = dto.Price;
                product.Stock = dto.Stock;
                await _context.SaveChangesAsync();
                return product;
            }
            return null;
        }
    }
}
