using Microsoft.EntityFrameworkCore.Storage;
using ShopNN.DTOs;
using ShopNN.Entities;
using ShopNN.Shared.Wrappers;

namespace ShopNN.Repositories.Interface
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task SaveChangesAsync();
        Task<List<Order>> GetByUserIdAsync(Guid UserId);
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task<PagedResult<Order>> GetPagedAsync(OrderQueryDTO query);
    }
}
