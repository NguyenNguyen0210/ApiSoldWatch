using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ShopNN.DTOs;
using ShopNN.Entities;
using ShopNN.Services.Interface;
using ShopNN.Exceptions;

namespace ShopNN.Services.Implement
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CategoryService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<CategoryResponseDTO> CreateAsync(CategoryRequestDTO dto)
        {
            var category = _mapper.Map<Category>(dto);
            category.Id = Guid.NewGuid();

            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            return _mapper.Map<CategoryResponseDTO>(category);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return false;

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<CategoryResponseDTO>> GetAllAsync()
        {
            var categories = await _context.Categories.AsNoTracking().ToListAsync();
            return _mapper.Map<List<CategoryResponseDTO>>(categories);
        }

        public async Task<CategoryResponseDTO> GetByIdAsync(Guid id)
        {
            var category = await _context.Categories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (category == null) throw new NotFoundException("Category not found");
            return _mapper.Map<CategoryResponseDTO>(category);
        }

        public async Task<CategoryResponseDTO> UpdateAsync(Guid id, CategoryRequestDTO dto)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) throw new NotFoundException("Category not found");

            _mapper.Map(dto, category);
            
            await _context.SaveChangesAsync();

            return _mapper.Map<CategoryResponseDTO>(category);
        }
    }
}
