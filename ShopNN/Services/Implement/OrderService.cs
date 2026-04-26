using Microsoft.EntityFrameworkCore;
using ShopNN.DTOs;
using ShopNN.Entities;
using ShopNN.Services.Interface;

namespace ShopNN.Services.Implement
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;

        public OrderService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<OrderDTO> CreateOrderAsync(Guid userId, List<OrderItemDTO> items)
        {
            if (items == null || !items.Any())
                throw new Exception("Order must have at least 1 item");

            var productIds = items.Select(i => i.ProductId).ToList();

            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            foreach (var item in items)
            {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);

                if (product == null)
                    throw new Exception($"Product {item.ProductId} not found");

                if (product.Stock < item.Quantity)
                    throw new Exception($"Product {product.Name} not enough stock");
            }

            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                Items = new List<OrderItem>()
            };

            foreach (var item in items)
            {
                var product = products.First(p => p.Id == item.ProductId);

                order.Items.Add(new OrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    Quantity = item.Quantity
                });

                product.Stock -= item.Quantity;
            }

            // transaction đảm bảo atomic
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                await _context.Orders.AddAsync(order);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
            OrderDTO result = new OrderDTO
            {
                Id = order.Id,
                CreatedAt = order.CreatedAt,
                Items = order.Items.Select(u => new OrderItemDTO
                {
                    ProductId = u.ProductId,
                    Quantity  = u.Quantity,
                }).ToList(),

            };
            return result;
        }

        // =========================
        // USER ORDERS
        // =========================
        public async Task<List<Order>> GetMyOrdersAsync(Guid userId)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        // =========================
        // ADMIN - ALL ORDERS
        // =========================
        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }
    }
}