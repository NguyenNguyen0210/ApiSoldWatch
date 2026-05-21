using AutoMapper;
using Microsoft.Extensions.Logging;
using ShopNN.DTOs;
using ShopNN.Entities;
using ShopNN.Repositories.Interface;
using ShopNN.Services.Interface;
using ShopNN.Shared.Exceptions;
using ShopNN.Shared.Wrappers;

namespace ShopNN.Services.Implement
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductService> _logger;

        public ProductService(IProductRepository productRepository, IMapper mapper, ILogger<ProductService> logger)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ProductResponseDTO> CreateAsync(ProductRequestDTO dto)
        {
            _logger.LogInformation("Creating new product with Name: {ProductName}, Price: {ProductPrice}", dto.Name, dto.Price);
            var product = _mapper.Map<Product>(dto);

            await _productRepository.AddAsync(product);
            _logger.LogInformation("Product created successfully with ID: {ProductId}", product.Id);

            return _mapper.Map<ProductResponseDTO>(product);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            _logger.LogInformation("Deleting product ID: {ProductId}", id);
            await _productRepository.DeleteAsync(id);
            _logger.LogInformation("Product ID: {ProductId} deleted successfully", id);
            return true;
        }

        public async Task<List<ProductResponseDTO>> GetAllAsync()
        {
            var products = await _productRepository.GetAllAsync();

            return _mapper.Map<List<ProductResponseDTO>>(products);
        }

        public async Task<PagedResult<ProductResponseDTO>> GetPagedAsync(ProductQueryDTO query)
        {
            var pagedProducts = await _productRepository.GetPagedAsync(query);

            return new PagedResult<ProductResponseDTO>
            {
                Items = _mapper.Map<List<ProductResponseDTO>>(pagedProducts.Items),
                Page = pagedProducts.Page,
                PageSize = pagedProducts.PageSize,
                TotalCount = pagedProducts.TotalCount
            };
        }

        public async Task<ProductResponseDTO> GetByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);

            if (product == null)
                throw new NotFoundException("Product not found");

            return _mapper.Map<ProductResponseDTO>(product);
        }

        public async Task<ProductResponseDTO> UpdateAsync(int id, ProductRequestDTO dto)
        {
            _logger.LogInformation("Updating product ID: {ProductId} with Name: {ProductName}", id, dto.Name);
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                _logger.LogWarning("Failed to update product. Product ID: {ProductId} not found", id);
                throw new NotFoundException("Product not found");
            }

            _mapper.Map(dto, product);
            await _productRepository.UpdateAsync(product);
            _logger.LogInformation("Product ID: {ProductId} updated successfully", id);

            return _mapper.Map<ProductResponseDTO>(product);
        }
    }
}
