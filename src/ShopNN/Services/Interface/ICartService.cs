using ShopNN.DTOs;

namespace ShopNN.Services.Interface
{
    public interface ICartService
    {
        Task<CartResponseDTO> GetCartByUserIdAsync(Guid userId);
        Task<CartResponseDTO> AddItemToCartAsync(Guid userId, CartItemRequestDTO dto);
        Task<CartResponseDTO> UpdateItemQuantityAsync(Guid userId, Guid cartItemId, CartItemUpdateDTO dto);
        Task<CartResponseDTO> RemoveItemFromCartAsync(Guid userId, Guid cartItemId);
        Task<bool> ClearCartAsync(Guid userId);
    }
}
