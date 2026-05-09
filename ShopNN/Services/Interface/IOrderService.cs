using ShopNN.DTOs;
using ShopNN.Entities;
using ShopNN.Shared.Enums;

namespace ShopNN.Services.Interface
{
    public interface IOrderService
    {
        Task<OrderResponseDTO> CreateOrderAsync(Guid userId, PaymentMethod paymentMethod);
        Task<List<OrderResponseDTO>> GetMyOrdersAsync(Guid userId);
        Task<List<OrderResponseDTO>> GetAllOrdersAsync(); // Admin
        Task<OrderResponseDTO> UpdateStatusAsync(Guid orderId, OrderStatus status);
    }
}
