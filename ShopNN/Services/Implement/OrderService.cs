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
                throw new ArgumentException("Order must have at least 1 item");

            var productIds = items.Select(i => i.ProductId).ToList();

            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id); // 🔥 tối ưu lookup

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var order = new Order
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    Items = new List<OrderItem>()
                };

                foreach (var item in items)
                {
                    if (!products.TryGetValue(item.ProductId, out var product))
                        throw new Exception($"Product {item.ProductId} not found");

                    if (product.Stock < item.Quantity)
                        throw new Exception($"Product {product.Name} not enough stock");

                    product.Stock -= item.Quantity;

                    order.Items.Add(new OrderItem
                    {
                        Id = Guid.NewGuid(),
                        ProductId = product.Id,
                        Quantity = item.Quantity
                    });
                }

                await _context.Orders.AddAsync(order);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return new OrderDTO
                {
                    Id = order.Id,
                    CreatedAt = order.CreatedAt,
                    Items = order.Items.Select(i => new OrderItemDTO
                    {
                        ProductId = i.ProductId,
                        Quantity = i.Quantity
                    }).ToList()
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // =========================
        // USER ORDERS
        // =========================
        public async Task<List<OrderDTO>> GetMyOrdersAsync(Guid userId)
        {
            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.Items)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return orders.Select(o => new OrderDTO
            {
                Id = o.Id,
                CreatedAt = o.CreatedAt,
                Items = o.Items.Select(i => new OrderItemDTO
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity
                }).ToList()
            }).ToList();
        }

        // =========================
        // ADMIN
        // =========================
        public async Task<List<OrderDTO>> GetAllOrdersAsync()
        {
            var orders = await _context.Orders
                .Include(o => o.Items)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return orders.Select(o => new OrderDTO
            {
                Id = o.Id,
                CreatedAt = o.CreatedAt,
                Items = o.Items.Select(i => new OrderItemDTO
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity
                }).ToList()
            }).ToList();
        }
    }
}