using Microsoft.EntityFrameworkCore;
using ShopNN.DTOs;
using ShopNN.Entities;
using ShopNN.Repositories.Interface;
using ShopNN.Shared.Exceptions;

namespace ShopNN.Repositories.Implement
{
    public class CartRepository : GenericRepository<Cart>, ICartRepository
    {
        private readonly ApplicationDbContext _context;
        public CartRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }


        public async Task ClearCartAsync(Guid CartId)
        {
            var items = await _context.CartItems
     .Where(ci => ci.CartId == CartId)
     .ToListAsync();
            _context.CartItems.RemoveRange(items);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteItemAsync(Guid itemId)
        {
            var item = await _context.CartItems.FindAsync(itemId)
                ?? throw new NotFoundException("Cart item not found");
            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();


        }

        public async override Task<Cart?> GetByIdAsync(object id)
        {
            Guid guidId = id is Guid g ? g : Guid.Parse(id.ToString()!);
            var cart = await _context.Carts
                .AsNoTracking()
                .Where(c => c.Id == guidId)
                .Include(c => c.Items)
                .ThenInclude(c => c.Product)
                .FirstOrDefaultAsync();
            return cart;
        }

        public async Task<Cart?> GetCartByUserIdAsync(Guid userId)
        {
            var cart = await _context.Carts
                .Where(c => c.UserId == userId)
                .Include (c => c.Items)
                .ThenInclude (c => c.Product)
                .FirstOrDefaultAsync();
            return cart;
        }

        public async Task<CartItem?> GetItemAsync(Guid cartItemId)
        {

            return await _context.CartItems
                 .Include(ci => ci.Product)
                 .FirstOrDefaultAsync(ci => ci.Id == cartItemId);
        }

        public async Task SaveChangeAsync()
        {
            await _context.SaveChangesAsync();
        }

    }
}
