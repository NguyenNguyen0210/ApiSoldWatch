using ShopNN.DTOs;

namespace ShopNN.Services.Interface
{
    public interface ICartService
    {
        Task<CartDTO> GetCartByUserIdAsync(Guid userId);
        Task<CartDTO> AddItemToCartAsync(Guid userId, AddToCartDTO dto);
        Task<CartDTO> UpdateItemQuantityAsync(Guid userId, Guid cartItemId, UpdateCartItemDTO dto);
        Task<CartDTO> RemoveItemFromCartAsync(Guid userId, Guid cartItemId);
        Task<bool> ClearCartAsync(Guid userId);
    }
}
