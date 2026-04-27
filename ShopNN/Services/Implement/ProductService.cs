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

        // =========================
        // CREATE
        // =========================
        public async Task<ProductResponseDTO> CreateAsync(ProductRequestDTO dto)
        {
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Stock = dto.Stock
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return MapToDTO(product);
        }

        // =========================
        // DELETE
        // =========================
        public async Task<bool> DeleteAsync(Guid id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return false;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return true;
        }

        // =========================
        // GET ALL
        // =========================
        public async Task<List<ProductResponseDTO>> GetAllAsync()
        {
            var products = await _context.Products
                .AsNoTracking()
                .ToListAsync();

            return products.Select(MapToDTO).ToList();
        }

        // =========================
        // GET BY ID
        // =========================
        public async Task<ProductResponseDTO?> GetByIdAsync(Guid id)
        {
            var product = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            return product == null ? null : MapToDTO(product);
        }

        // =========================
        // UPDATE
        // =========================
        public async Task<ProductResponseDTO?> UpdateAsync(Guid id, ProductRequestDTO dto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return null;

            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.Stock = dto.Stock;

            await _context.SaveChangesAsync();

            return MapToDTO(product);
        }

        // =========================
        // MAPPING
        // =========================
        private static ProductResponseDTO MapToDTO(Product p)
        {
            return new ProductResponseDTO
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock
            };
        }
    }
}