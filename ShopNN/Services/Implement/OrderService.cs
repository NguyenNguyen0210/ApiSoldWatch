using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ShopNN.DTOs;
using ShopNN.Entities;
using ShopNN.Repositories.Implement;
using ShopNN.Repositories.Interface;
using ShopNN.Services.Interface;
using ShopNN.Shared.Enums;
using ShopNN.Shared.Exeptions;

namespace ShopNN.Services.Implement
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public OrderService(
            IOrderRepository orderRepository,
            ICartRepository cartRepository,
            IProductRepository productRepository,
            IMapper mapper)
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<OrderResponseDTO> CreateOrderAsync(Guid userId, PaymentMethod paymentMethod)
        {
            var cart = await _cartRepository.GetCartByUserIdAsync(userId);

            if (cart == null || !cart.Items.Any())
                throw new BadRequestException("Your cart is empty.");
            using var transaction = await _orderRepository.BeginTransactionAsync();
            try
            {
                var order = new Order
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    Status = OrderStatus.Pending,
                    PaymentMethod = paymentMethod,
                    PaymentStatus = PaymentStatus.Unpaid,
                    TotalAmount = 0,
                    Items = new List<OrderItem>()
                };

                foreach (var cartItem in cart.Items)
                {
                    var product = cartItem.Product
                        ?? throw new NotFoundException("Product not found.");

                    if (product.Stock < cartItem.Quantity)
                        throw new BadRequestException($"Product '{product.Name}' is out of stock.");

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

                cart.Items.Clear();
                cart.UpdatedAt = DateTime.UtcNow;

                await _orderRepository.AddAsync(order);
                await _orderRepository.SaveChangesAsync();
                await transaction.CommitAsync();

                return _mapper.Map<OrderResponseDTO>(order);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<OrderResponseDTO>> GetMyOrdersAsync(Guid userId)
        {
            var orders = await _orderRepository.GetByUserIdAsync(userId);
            return _mapper.Map<List<OrderResponseDTO>>(orders);
        }

        public async Task<List<OrderResponseDTO>> GetAllOrdersAsync()
        {
            var orders = await _orderRepository.GetAllAsync();
            return _mapper.Map<List<OrderResponseDTO>>(orders);
        }

        public async Task<OrderResponseDTO> UpdateStatusAsync(Guid orderId, OrderStatus status)
        {
            var order = await _orderRepository.GetByIdAsync(orderId)
                ?? throw new NotFoundException("Order not found.");

            order.Status = status;
            await _orderRepository.SaveChangesAsync();

            return _mapper.Map<OrderResponseDTO>(order);
        }
    }
}