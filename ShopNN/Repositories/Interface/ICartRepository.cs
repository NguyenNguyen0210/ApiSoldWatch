using ShopNN.Entities;

namespace ShopNN.Repositories.Interface
{
    public interface ICartRepository
    {
        Task<Cart?> GetByIdAsync(Guid id);
        Task<Cart> AddAsync(Cart data);
        Task<Cart?> GetCartByUserIdAsync(Guid userId);
        Task<Cart?> GetCartForUpdateAsync(Guid userId);
        Task DeleteItemAsync(Guid itemId);
        Task<CartItem?> GetItemAsync(Guid cartItemId);
        Task ClearCartAsync(Guid CartId);
        Task SaveChangeAsync();
    }
}
