using ShopNN.DTOs;
using ShopNN.Entities;
using ShopNN.Shared.Wrappers;

namespace ShopNN.Repositories.Interface
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<PagedResult<Product>> GetPagedAsync(ProductQueryDTO query);
    }
}
