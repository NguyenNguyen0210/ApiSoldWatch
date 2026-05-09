using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ShopNN.DTOs;
using ShopNN.Entities;
using ShopNN.Repositories.Implement;
using ShopNN.Repositories.Interface;
using ShopNN.Services.Interface;
using ShopNN.Shared.Exeptions;

namespace ShopNN.Services.Implement
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRespository _categoryRepository;
        private readonly IMapper _mapper;

        public CategoryService(ICategoryRespository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<CategoryResponseDTO> CreateAsync(CategoryRequestDTO dto)
        {
            var category = _mapper.Map<Category>(dto);
            category.Id = Guid.NewGuid();

            await _categoryRepository.AddAsync(category);

            return _mapper.Map<CategoryResponseDTO>(category);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            await _categoryRepository.DeleteAsync(id);
            return true;
        }

        public async Task<List<CategoryResponseDTO>> GetAllAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return _mapper.Map<List<CategoryResponseDTO>>(categories);
        }

        public async Task<CategoryResponseDTO> GetByIdAsync(Guid id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) throw new NotFoundException("Category not found");
            return _mapper.Map<CategoryResponseDTO>(category);
        }

        public async Task<CategoryResponseDTO> UpdateAsync(Guid id, CategoryRequestDTO dto)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) throw new NotFoundException("Category not found");

            _mapper.Map(dto, category);
            
            await _categoryRepository.UpdateAsync(category);

            return _mapper.Map<CategoryResponseDTO>(category);
        }
    }
}
