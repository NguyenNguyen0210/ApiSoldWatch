using ShopNN.Entities;

namespace ShopNN.Repositories.Interface
{
    public interface IPaymentRepository
    {
        Task<Payment> AddAsync(Payment data);
        Task<Payment?> GetByOrderIdAsync(Guid orderId);
        Task SaveChangesAsync();
    }
}
