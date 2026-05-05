using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ShopNN.DTOs;
using ShopNN.Entities;
using ShopNN.Services.Interface;
using ShopNN.Exceptions;

namespace ShopNN.Services.Implement
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CartService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        private async Task<Cart> GetOrCreateCartAsync(Guid userId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { Id = Guid.NewGuid(), UserId = userId };
                await _context.Carts.AddAsync(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        public async Task<CartDTO> GetCartByUserIdAsync(Guid userId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null) return await GetCartByUserIdAsync(userId); // Fallback to create if not exists
            
            return _mapper.Map<CartDTO>(cart);
        }

        public async Task<CartDTO> AddItemToCartAsync(Guid userId, AddToCartDTO dto)
        {
            var cart = await GetOrCreateCartAsync(userId);
            
            // Check if product exists
            var productExists = await _context.Products.AnyAsync(p => p.Id == dto.ProductId);
            if (!productExists) throw new NotFoundException("Product not found");

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == dto.ProductId);

            if (existingItem != null)
            {
                existingItem.Quantity += dto.Quantity;
            }
            else
            {
                var newItem = new CartItem
                {
                    Id = Guid.NewGuid(),
                    CartId = cart.Id,
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity
                };
                cart.Items.Add(newItem);
            }

            cart.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Return current state
            return await GetCartByUserIdAsync(userId);
        }

        public async Task<CartDTO> UpdateItemQuantityAsync(Guid userId, Guid cartItemId, UpdateCartItemDTO dto)
        {
            var cart = await GetOrCreateCartAsync(userId);
            var item = cart.Items.FirstOrDefault(i => i.Id == cartItemId);

            if (item == null) throw new NotFoundException("Cart item not found");

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

            return await GetCartByUserIdAsync(userId);
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

            return await GetCartByUserIdAsync(userId);
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
