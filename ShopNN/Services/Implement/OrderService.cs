using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ShopNN.DTOs;
using ShopNN.Entities;
using ShopNN.Exceptions;
using ShopNN.Services.Interface;

namespace ShopNN.Services.Implement
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public OrderService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<OrderDTO> CreateOrderAsync(Guid userId, PaymentMethod paymentMethod)
        {
            // 1. Get user's cart
            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.Items.Any())
                throw new BadRequestException("Your cart is empty");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var order = new Order
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    TotalAmount = 0,
                    Status = OrderStatus.Pending,
                    PaymentMethod = paymentMethod,
                    PaymentStatus = PaymentStatus.Unpaid, // Default
                    Items = new List<OrderItem>()
                };

                foreach (var cartItem in cart.Items)
                {
                    var product = cartItem.Product;
                    if (product == null)
                        throw new NotFoundException($"Product in cart not found");

                    if (product.Stock < cartItem.Quantity)
                        throw new BadRequestException($"Product '{product.Name}' does not have enough stock (Available: {product.Stock})");

                    // Reduce stock
                    product.Stock -= cartItem.Quantity;

                    var orderItem = new OrderItem
                    {
                        Id = Guid.NewGuid(),
                        OrderId = order.Id,
                        ProductId = product.Id,
                        Quantity = cartItem.Quantity,
                        UnitPrice = product.Price 
                    };

                    order.Items.Add(orderItem);
                    order.TotalAmount += orderItem.UnitPrice * orderItem.Quantity;
                }

                // 2. Save Order
                await _context.Orders.AddAsync(order);

                // 3. Clear Cart
                _context.CartItems.RemoveRange(cart.Items);
                cart.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Reload order with product details for the DTO
                var resultOrder = await _context.Orders
                    .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(o => o.Id == order.Id);

                return _mapper.Map<OrderDTO>(resultOrder);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<OrderDTO>> GetMyOrdersAsync(Guid userId)
        {
            var orders = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

            return _mapper.Map<List<OrderDTO>>(orders);
        }

        public async Task<List<OrderDTO>> GetAllOrdersAsync()
        {
            var orders = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .OrderByDescending(o => o.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

            return _mapper.Map<List<OrderDTO>>(orders);
        }

        public async Task<OrderDTO> UpdateStatusAsync(Guid orderId, OrderStatus status)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                throw new NotFoundException("Order not found");

            order.Status = status;
            await _context.SaveChangesAsync();

            return _mapper.Map<OrderDTO>(order);
        }
    }
}