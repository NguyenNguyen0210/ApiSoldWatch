using ShopNN.DTOs.Account;
using ShopNN.DTOs.Product;
using ShopNN.DTOs.Category;
using ShopNN.DTOs.Cart;
using ShopNN.DTOs.Order;
using ShopNN.Entities;
using ShopNN.Shared.Enums;
using ShopNN.Shared.Wrappers;

namespace ShopNN.Services.Interface
{
    public interface IOrderService
    {
        Task<OrderResponseDTO> CreateOrderAsync(Guid userId, OrderCreateRequestDTO request);
        Task<List<OrderResponseDTO>> GetMyOrdersAsync(Guid userId);
        Task<List<OrderResponseDTO>> GetAllOrdersAsync();
        Task<PagedResult<OrderResponseDTO>> GetAllOrdersPagedAsync(OrderQueryDTO query);
        Task<OrderResponseDTO> UpdateStatusAsync(Guid orderId, OrderStatus status);
        Task<OrderResponseDTO> UpdatePaymentStatusAsync(Guid orderId, PaymentStatus paymentStatus);
    }
}

