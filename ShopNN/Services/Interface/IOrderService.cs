using ShopNN.DTOs;

namespace ShopNN.Services.Interface
{
    public interface IOrderService
    {
        Task<OrderDTO>  CreateOrderAsync(Guid userId, List<OrderItemDTO> items);

        Task<List<Order>> GetMyOrdersAsync(Guid userId);

        Task<List<Order>> GetAllOrdersAsync(); // Admin
    }
}
