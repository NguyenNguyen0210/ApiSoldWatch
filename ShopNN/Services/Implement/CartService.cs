using Microsoft.EntityFrameworkCore;
using ShopNN.DTOs;
using ShopNN.Entities;
using ShopNN.Services.Interface;

namespace ShopNN.Services.Implement
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;

        public CartService(ApplicationDbContext context)
        {
            _context = context;
        }

        private async Task<Cart> GetOrCreateCartAsync(Guid userId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { Id = Guid.NewGuid(), UserId = userId };
                await _context.Carts.AddAsync(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        private CartDTO MapToDTO(Cart cart)
        {
            return new CartDTO
            {
                Id = cart.Id,
                UserId = cart.UserId,
                Items = cart.Items.Select(i => new CartItemDTO
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product?.Name,
                    ProductPrice = i.Product?.Price ?? 0,
                    Quantity = i.Quantity
                }).ToList()
            };
        }

        public async Task<CartDTO> GetCartByUserIdAsync(Guid userId)
        {
            var cart = await GetOrCreateCartAsync(userId);
            return MapToDTO(cart);
        }

        public async Task<CartDTO> AddItemToCartAsync(Guid userId, AddToCartDTO dto)
        {
            var cart = await GetOrCreateCartAsync(userId);
            var product = await _context.Products.FindAsync(dto.ProductId);

            if (product == null) throw new Exception("Product not found");

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == dto.ProductId);

            if (existingItem != null)
            {
                existingItem.Quantity += dto.Quantity;
            }
            else
            {
                cart.Items.Add(new CartItem
                {
                    Id = Guid.NewGuid(),
                    CartId = cart.Id,
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity
                });
            }

            cart.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Reload to get Product details for DTO
            var updatedCart = await GetOrCreateCartAsync(userId);
            return MapToDTO(updatedCart);
        }

        public async Task<CartDTO> UpdateItemQuantityAsync(Guid userId, Guid cartItemId, UpdateCartItemDTO dto)
        {
            var cart = await GetOrCreateCartAsync(userId);
            var item = cart.Items.FirstOrDefault(i => i.Id == cartItemId);

            if (item == null) throw new Exception("Cart item not found");

            if (dto.Quantity <= 0)
            {
                _context.CartItems.Remove(item);
            }
            else
            {
                item.Quantity = dto.Quantity;
            }

            cart.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var updatedCart = await GetOrCreateCartAsync(userId);
            return MapToDTO(updatedCart);
        }

        public async Task<CartDTO> RemoveItemFromCartAsync(Guid userId, Guid cartItemId)
        {
            var cart = await GetOrCreateCartAsync(userId);
            var item = cart.Items.FirstOrDefault(i => i.Id == cartItemId);

            if (item != null)
            {
                _context.CartItems.Remove(item);
                cart.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            var updatedCart = await GetOrCreateCartAsync(userId);
            return MapToDTO(updatedCart);
        }

        public async Task<bool> ClearCartAsync(Guid userId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart != null && cart.Items.Any())
            {
                _context.CartItems.RemoveRange(cart.Items);
                cart.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return true;
        }
    }
}
