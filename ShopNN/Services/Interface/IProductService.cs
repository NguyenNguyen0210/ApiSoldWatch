using ShopNN.DTOs;
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
