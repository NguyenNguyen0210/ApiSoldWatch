using Microsoft.EntityFrameworkCore;
using ShopNN.DTOs;
using ShopNN.Entities;
using ShopNN.Repositories.Interface;
using ShopNN.Shared.Exceptions;

namespace ShopNN.Repositories.Implement
{
    public class CartRepository : ICartRepository
    {
        private readonly ApplicationDbContext _context;
        public CartRepository(ApplicationDbContext context)
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

        public async Task<Cart?> GetByIdAsync(Guid id)
        {
            var cart = await _context.Carts
                .AsNoTracking()
                .Where(c => c.Id == id)
                .Include(c => c.Items)
                .ThenInclude(c => c.Product)
                .FirstOrDefaultAsync();
            return cart;
        }

        public async Task<Cart> AddAsync(Cart data)
        {
            _context.Carts.Add(data);
            await _context.SaveChangesAsync();
            return data;
        }

        /// <summary>
        /// Read-only: loads cart with Items + Product for response mapping (AsNoTracking).
        /// </summary>
        public async Task<Cart?> GetCartByUserIdAsync(Guid userId)
        {
            var cart = await _context.Carts
                .AsNoTracking()
                .Where(c => c.UserId == userId)
                .Include(c => c.Items)
                .ThenInclude(c => c.Product)
                .FirstOrDefaultAsync();
            return cart;
        }

        /// <summary>
        /// Write-ready: loads cart with Items for tracking (WITHOUT .ThenInclude(Product)
        /// to avoid tracking conflicts with ProductRepository).
        /// </summary>
        public async Task<Cart?> GetCartForUpdateAsync(Guid userId)
        {
            var cart = await _context.Carts
                .Where(c => c.UserId == userId)
                .Include(c => c.Items)
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
