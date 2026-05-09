using Microsoft.EntityFrameworkCore.Storage;

namespace ShopNN.Repositories.Interface
{
    public interface IOrderRepository: IRepository<Order>
    {
        Task SaveChangesAsync();
        Task<List<Order>> GetByUserIdAsync(Guid UserId);
        Task<IDbContextTransaction> BeginTransactionAsync();

    }
}
