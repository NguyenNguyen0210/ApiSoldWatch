using ShopNN.DTOs.Account;
using ShopNN.DTOs.Product;
using ShopNN.DTOs.Category;
using ShopNN.DTOs.Cart;
using ShopNN.DTOs.Order;
using ShopNN.Entities;
using ShopNN.Shared.Wrappers;

namespace ShopNN.Repositories.Interface
{
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(int id);
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product> AddAsync(Product data);
        Task UpdateAsync(Product data);
        Task DeleteAsync(int id);
        Task<PagedResult<Product>> GetPagedAsync(ProductQueryDTO query);
    }
}

