using ShopNN.DTOs.Account;
using ShopNN.DTOs.Product;
using ShopNN.DTOs.Category;
using ShopNN.DTOs.Cart;
using ShopNN.DTOs.Order;

namespace ShopNN.Services.Interface
{
    public interface ICategoryService
    {
        Task<List<CategoryResponseDTO>> GetAllAsync();
        Task<CategoryResponseDTO> GetByIdAsync(int id);
        Task<CategoryResponseDTO> CreateAsync(CategoryRequestDTO dto);
        Task<CategoryResponseDTO> UpdateAsync(int id, CategoryRequestDTO dto);
        Task<bool> DeleteAsync(int id);
    }
}

