using AutoMapper;
using Microsoft.Extensions.Logging;
using ShopNN.DTOs.Account;
using ShopNN.DTOs.Product;
using ShopNN.DTOs.Category;
using ShopNN.DTOs.Cart;
using ShopNN.DTOs.Order;
using ShopNN.Entities;
using ShopNN.Repositories.Interface;
using ShopNN.Services.Interface;
using ShopNN.Shared.Enums;
using ShopNN.Shared.Exceptions;
using ShopNN.Shared.Wrappers;

namespace ShopNN.Services.Implement
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IOrderRepository orderRepository,
            ICartRepository cartRepository,
            IProductRepository productRepository,
            IMapper mapper,
            ILogger<OrderService> logger)
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<OrderResponseDTO> CreateOrderAsync(Guid userId, OrderCreateRequestDTO request)
        {
            _logger.LogInformation("Starting order creation process for User ID: {UserId} with PaymentMethod: {PaymentMethod}", userId, request.PaymentMethod);
            var cart = await _cartRepository.GetCartByUserIdAsync(userId);

            if (cart == null || !cart.Items.Any())
            {
                _logger.LogWarning("Checkout failed for User ID: {UserId}. Cart is empty.", userId);
                throw new BadRequestException("Your cart is empty.");
            }

            _logger.LogInformation("Cart found with {ItemCount} items. Beginning database transaction...", cart.Items.Count);
            using var transaction = await _orderRepository.BeginTransactionAsync();
            try
            {
                var order = new Order
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    Status = OrderStatus.Pending,
                    PaymentMethod = request.PaymentMethod,
                    PaymentStatus = PaymentStatus.Unpaid,
                    ReceiverName = request.ReceiverName,
                    PhoneNumber = request.PhoneNumber,
                    ShippingAddress = request.ShippingAddress,
                    TotalAmount = 0,
                    Items = new List<OrderItem>()
                };

                foreach (var cartItem in cart.Items)
                {
                    var product = cartItem.Product
                        ?? throw new NotFoundException("Product not found.");

                    _logger.LogDebug("Checking stock for Product ID: {ProductId}. Requested: {Qty}, Available: {Stock}", product.Id, cartItem.Quantity, product.Stock);
                    if (product.Stock < cartItem.Quantity)
                    {
                        _logger.LogWarning("Product ID: {ProductId} ({ProductName}) is out of stock. Stock: {Stock}, Requested: {Qty}", product.Id, product.Name, product.Stock, cartItem.Quantity);
                        throw new BadRequestException($"Product '{product.Name}' is out of stock.");
                    }

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

                if (request.PaymentMethod == PaymentMethod.COD)
                {
                    _logger.LogInformation("COD order. Clearing items from Cart ID: {CartId}...", cart.Id);
                    await _cartRepository.ClearCartAsync(cart.Id);
                }
                else
                {
                    _logger.LogInformation("Online payment method ({PaymentMethod}). Cart ID: {CartId} will be cleared upon successful payment confirmation.", request.PaymentMethod, cart.Id);
                }

                await _orderRepository.AddAsync(order);
                await _orderRepository.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Successfully created Order ID: {OrderId} for User ID: {UserId} with TotalAmount: {TotalAmount}", order.Id, userId, order.TotalAmount);
                return _mapper.Map<OrderResponseDTO>(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create order for User ID: {UserId} due to an error: {Message}. Transaction rolled back.", userId, ex.Message);
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

        public async Task<PagedResult<OrderResponseDTO>> GetAllOrdersPagedAsync(OrderQueryDTO query)
        {
            var pagedOrders = await _orderRepository.GetPagedAsync(query);

            return new PagedResult<OrderResponseDTO>
            {
                Items = _mapper.Map<List<OrderResponseDTO>>(pagedOrders.Items),
                Page = pagedOrders.Page,
                PageSize = pagedOrders.PageSize,
                TotalCount = pagedOrders.TotalCount
            };
        }

        public async Task<OrderResponseDTO> UpdateStatusAsync(Guid orderId, OrderStatus status)
        {
            _logger.LogInformation("Updating status of Order ID: {OrderId} to {NewStatus}", orderId, status);
            var order = await _orderRepository.GetByIdAsync(orderId)
                ?? throw new NotFoundException("Order not found.");

            var oldStatus = order.Status;
            order.Status = status;

            // Business logic for COD & overall:
            // When status is marked as Delivered, automatically mark PaymentStatus as Paid
            if (status == OrderStatus.Delivered && order.PaymentStatus == PaymentStatus.Unpaid)
            {
                _logger.LogInformation("Order ID: {OrderId} status changed to Delivered. Automatically updating PaymentStatus from Unpaid to Paid", orderId);
                order.PaymentStatus = PaymentStatus.Paid;
            }

            await _orderRepository.SaveChangesAsync();
            _logger.LogInformation("Successfully updated Order ID: {OrderId} status from {OldStatus} to {NewStatus}", orderId, oldStatus, status);

            return _mapper.Map<OrderResponseDTO>(order);
        }

        public async Task<OrderResponseDTO> UpdatePaymentStatusAsync(Guid orderId, PaymentStatus paymentStatus)
        {
            _logger.LogInformation("Updating payment status of Order ID: {OrderId} to {NewPaymentStatus}", orderId, paymentStatus);
            var order = await _orderRepository.GetByIdAsync(orderId)
                ?? throw new NotFoundException("Order not found.");

            var oldPaymentStatus = order.PaymentStatus;
            order.PaymentStatus = paymentStatus;
            await _orderRepository.SaveChangesAsync();
            _logger.LogInformation("Successfully updated Order ID: {OrderId} payment status from {OldPaymentStatus} to {NewPaymentStatus}", orderId, oldPaymentStatus, paymentStatus);

            return _mapper.Map<OrderResponseDTO>(order);
        }
    }
}
