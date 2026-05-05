using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ShopNN.DTOs;
using ShopNN.Entities;
using ShopNN.Services.Interface;
using ShopNN.Exceptions;

namespace ShopNN.Services.Implement
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ProductService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ProductResponseDTO> CreateAsync(ProductRequestDTO dto)
        {
            var product = _mapper.Map<Product>(dto);
            product.Id = Guid.NewGuid();

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return _mapper.Map<ProductResponseDTO>(product);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return false;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<ProductResponseDTO>> GetAllAsync()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .AsNoTracking()
                .ToListAsync();

            return _mapper.Map<List<ProductResponseDTO>>(products);
        }

        public async Task<ProductResponseDTO> GetByIdAsync(Guid id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (product == null)
                throw new NotFoundException("Product not found");

            return _mapper.Map<ProductResponseDTO>(product);
        }

        public async Task<ProductResponseDTO> UpdateAsync(Guid id, ProductRequestDTO dto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                throw new NotFoundException("Product not found");

            _mapper.Map(dto, product);

            await _context.SaveChangesAsync();

            // Fetch again with Category for the response
            var updatedProduct = await _context.Products
                .Include(p => p.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            return _mapper.Map<ProductResponseDTO>(updatedProduct);
        }
    }
}