using ShopNN.DTOs;

namespace ShopNN.Services.Interface
{
    public interface IOrderService
    {
        Task<OrderDTO>  CreateOrderAsync(Guid userId, List<OrderItemDTO> items);

        Task<List<OrderDTO>> GetMyOrdersAsync(Guid userId);

        Task<List<OrderDTO>> GetAllOrdersAsync(); // Admin
    }
}
