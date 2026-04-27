using ShopNN.DTOs;

namespace ShopNN.Services.Interface
{
    public interface IProductService
    {
        Task<List<ProductResponseDTO>> GetAllAsync();
        Task<ProductResponseDTO?> GetByIdAsync(Guid id);

        Task<ProductResponseDTO> CreateAsync(ProductRequestDTO dto);
        Task<ProductResponseDTO> UpdateAsync(Guid id, ProductRequestDTO dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
