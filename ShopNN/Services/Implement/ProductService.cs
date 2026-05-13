using AutoMapper;
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

        public ProductService(IProductRepository productRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<ProductResponseDTO> CreateAsync(ProductRequestDTO dto)
        {
            var product = _mapper.Map<Product>(dto);

            await _productRepository.AddAsync(product);

            return _mapper.Map<ProductResponseDTO>(product);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            await _productRepository.DeleteAsync(id);
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
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                throw new NotFoundException("Product not found");

            _mapper.Map(dto, product);
            await _productRepository.UpdateAsync(product);

            return _mapper.Map<ProductResponseDTO>(product);
        }
    }
}
