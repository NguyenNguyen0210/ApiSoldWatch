using Microsoft.EntityFrameworkCore;
using ShopNN.DTOs;
using ShopNN.Entities;
using ShopNN.Services.Interface;

namespace ShopNN.Services.Implement
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;

        public CategoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CategoryDTO> CreateAsync(CategoryRequestDTO dto)
        {
            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = dto.Name
            };

            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            return new CategoryDTO { Id = category.Id, Name = category.Name };
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return false;

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<CategoryDTO>> GetAllAsync()
        {
            var categories = await _context.Categories.ToListAsync();
            return categories.Select(c => new CategoryDTO { Id = c.Id, Name = c.Name }).ToList();
        }

        public async Task<CategoryDTO> GetByIdAsync(Guid id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return null;

            return new CategoryDTO { Id = category.Id, Name = category.Name };
        }

        public async Task<CategoryDTO> UpdateAsync(Guid id, CategoryRequestDTO dto)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return null;

            category.Name = dto.Name;
            
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();

            return new CategoryDTO { Id = category.Id, Name = category.Name };
        }
    }
}
