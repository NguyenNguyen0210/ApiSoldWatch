using Microsoft.EntityFrameworkCore.Storage;
using ShopNN.DTOs;
using ShopNN.Entities;
using ShopNN.Shared.Wrappers;

namespace ShopNN.Repositories.Interface
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(Guid id);
        Task<IEnumerable<Order>> GetAllAsync();
        Task<Order> AddAsync(Order data);
        Task SaveChangesAsync();
        Task<List<Order>> GetByUserIdAsync(Guid UserId);
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task<PagedResult<Order>> GetPagedAsync(OrderQueryDTO query);
    }
}
