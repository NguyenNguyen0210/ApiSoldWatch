using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShopNN.DTOs.Account;
using ShopNN.DTOs.Product;
using ShopNN.DTOs.Category;
using ShopNN.DTOs.Cart;
using ShopNN.DTOs.Order;
using ShopNN.Entities;
using ShopNN.Repositories.Implement;
using ShopNN.Repositories.Interface;
using ShopNN.Services.Interface;
using ShopNN.Shared.Exceptions;

namespace ShopNN.Services.Implement
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(ICategoryRepository categoryRepository, IMapper mapper, ILogger<CategoryService> logger)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<CategoryResponseDTO> CreateAsync(CategoryRequestDTO dto)
        {
            _logger.LogInformation("Creating new category with Name: {CategoryName}", dto.Name);
            var category = _mapper.Map<Category>(dto);

            await _categoryRepository.AddAsync(category);
            _logger.LogInformation("Category created successfully with ID: {CategoryId}", category.Id);

            return _mapper.Map<CategoryResponseDTO>(category);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            _logger.LogInformation("Deleting category ID: {CategoryId}", id);
            await _categoryRepository.DeleteAsync(id);
            _logger.LogInformation("Category ID: {CategoryId} deleted successfully", id);
            return true;
        }

        public async Task<List<CategoryResponseDTO>> GetAllAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return _mapper.Map<List<CategoryResponseDTO>>(categories);
        }

        public async Task<CategoryResponseDTO> GetByIdAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) throw new NotFoundException("Category not found");
            return _mapper.Map<CategoryResponseDTO>(category);
        }

        public async Task<CategoryResponseDTO> UpdateAsync(int id, CategoryRequestDTO dto)
        {
            _logger.LogInformation("Updating category ID: {CategoryId} with Name: {CategoryName}", id, dto.Name);
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                _logger.LogWarning("Failed to update category. Category ID: {CategoryId} not found", id);
                throw new NotFoundException("Category not found");
            }

            _mapper.Map(dto, category);
            
            await _categoryRepository.UpdateAsync(category);
            _logger.LogInformation("Category ID: {CategoryId} updated successfully", id);

            return _mapper.Map<CategoryResponseDTO>(category);
        }
    }
}

