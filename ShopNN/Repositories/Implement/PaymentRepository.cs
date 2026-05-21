using Microsoft.EntityFrameworkCore;
using ShopNN.Entities;
using ShopNN.Repositories.Interface;
using System.Linq.Expressions;

namespace ShopNN.Repositories.Implement
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly ApplicationDbContext _context;
        public PaymentRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<Payment> AddAsync(Payment data)
        {
            _context.Payments.Add(data);
            await _context.SaveChangesAsync();
            return data;
        }
        public async Task<Payment?> GetByOrderIdAsync(Guid orderId)
        {
            return await _context.Payments
        .FirstOrDefaultAsync(p => p.OrderId == orderId);
        }
        
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

    }
}
