using ShopNN.Entities;

namespace ShopNN.Repositories.Interface
{
    public interface ICartRepository:IRepository<Cart>
    {
        Task<Cart?> GetCartByUserIdAsync(Guid userId); 
        Task DeleteItemAsync(Guid itemId);
        Task<CartItem?> GetItemAsync(Guid cartItemId);

        Task ClearCartAsync(Guid CartId);
        Task SaveChangeAsync();
    }
}
