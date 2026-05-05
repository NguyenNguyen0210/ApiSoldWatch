using ShopNN.DTOs;

namespace ShopNN.Services.Interface
{
    public interface IOrderService
    {
        Task<OrderDTO> CreateOrderAsync(Guid userId);

        Task<List<OrderDTO>> GetMyOrdersAsync(Guid userId);

        Task<List<OrderDTO>> GetAllOrdersAsync(); // Admin
        Task<OrderDTO> UpdateStatusAsync(Guid orderId, OrderStatus status);
    }
}
