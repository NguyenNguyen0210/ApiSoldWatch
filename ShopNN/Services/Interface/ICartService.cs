using ShopNN.DTOs.Account;
using ShopNN.DTOs.Product;
using ShopNN.DTOs.Category;
using ShopNN.DTOs.Cart;
using ShopNN.DTOs.Order;

namespace ShopNN.Services.Interface
{
    public interface ICartService
    {
        Task<CartResponseDTO> GetCartByUserIdAsync(Guid userId);
        Task<CartResponseDTO> GetCartByIdAsync(Guid id);
        Task<CartResponseDTO> AddItemToCartAsync(Guid userId, CartItemRequestDTO dto);
        Task<CartResponseDTO> UpdateItemQuantityAsync(Guid userId, Guid cartItemId, CartItemUpdateDTO dto);
        Task<CartResponseDTO> RemoveItemFromCartAsync(Guid userId, Guid cartItemId);
        Task ClearCartAsync(Guid userId);
    }
}

