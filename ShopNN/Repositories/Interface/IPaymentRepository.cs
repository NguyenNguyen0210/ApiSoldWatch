using ShopNN.Entities;

namespace ShopNN.Repositories.Interface
{
    public interface IPaymentRepository: IRepository<Payment>
    {
        Task<Payment?> GetByOrderIdAsync(Guid orderId);
        Task SaveChangesAsync();
    }
}
