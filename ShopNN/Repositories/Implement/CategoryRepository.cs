using Microsoft.EntityFrameworkCore;
using ShopNN.Entities;
using ShopNN.Repositories.Interface;
using ShopNN.Shared.Exceptions;

namespace ShopNN.Repositories.Implement
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;
        public CategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _context.Categories.FindAsync(id);
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _context.Categories.AsNoTracking().ToListAsync();
        }

        public async Task<Category> AddAsync(Category data)
        {
            _context.Categories.Add(data);
            await _context.SaveChangesAsync();
            return data;
        }

        public async Task UpdateAsync(Category data)
        {
            _context.Categories.Update(data);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var data = await GetByIdAsync(id) ?? throw new NotFoundException($"Category with id {id} not found.");
            _context.Categories.Remove(data);
            await _context.SaveChangesAsync();
        }
    }
}
