using ShopNN.DTOs.Account;
using ShopNN.DTOs.Product;
using ShopNN.DTOs.Category;
using ShopNN.DTOs.Cart;
using ShopNN.DTOs.Order;
using ShopNN.Shared.Wrappers;

namespace ShopNN.Services.Interface
{
    public interface IProductService
    {
        Task<List<ProductResponseDTO>> GetAllAsync();
        Task<PagedResult<ProductResponseDTO>> GetPagedAsync(ProductQueryDTO query);
        Task<ProductResponseDTO> GetByIdAsync(int id);

        Task<ProductResponseDTO> CreateAsync(ProductRequestDTO dto);
        Task<ProductResponseDTO> UpdateAsync(int id, ProductRequestDTO dto);
        Task<bool> DeleteAsync(int id);
    }
}

