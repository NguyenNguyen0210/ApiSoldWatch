using ShopNN.DTOs;

namespace ShopNN.Services.Interface
{
    public interface IProductService
    {
        Task<List<Product>> GetAllAsync();
        Task<Product?> GetByIdAsync(Guid id);

        Task<Product> CreateAsync(ProductDTO dto);
        Task<Product> UpdateAsync(Guid id, ProductDTO dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
