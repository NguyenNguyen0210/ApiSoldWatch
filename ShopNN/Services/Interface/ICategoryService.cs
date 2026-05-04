using ShopNN.DTOs;

namespace ShopNN.Services.Interface
{
    public interface ICategoryService
    {
        Task<List<CategoryDTO>> GetAllAsync();
        Task<CategoryDTO> GetByIdAsync(Guid id);
        Task<CategoryDTO> CreateAsync(CategoryRequestDTO dto);
        Task<CategoryDTO> UpdateAsync(Guid id, CategoryRequestDTO dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
