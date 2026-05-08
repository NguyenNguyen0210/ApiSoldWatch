using ShopNN.DTOs;

namespace ShopNN.Services.Interface
{
    public interface ICategoryService
    {
        Task<List<CategoryResponseDTO>> GetAllAsync();
        Task<CategoryResponseDTO> GetByIdAsync(Guid id);
        Task<CategoryResponseDTO> CreateAsync(CategoryRequestDTO dto);
        Task<CategoryResponseDTO> UpdateAsync(Guid id, CategoryRequestDTO dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
