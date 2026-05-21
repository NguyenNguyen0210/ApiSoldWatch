using ShopNN.Entities;

namespace ShopNN.Repositories.Interface
{
    public interface ICategoryRepository
    {
        Task<Category?> GetByIdAsync(int id);
        Task<IEnumerable<Category>> GetAllAsync();
        Task<Category> AddAsync(Category data);
        Task UpdateAsync(Category data);
        Task DeleteAsync(int id);
    }
}
