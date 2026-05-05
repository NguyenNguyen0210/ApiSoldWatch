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

        private async Task<Cart> GetActiveCartAsync(Guid userId, bool includeProducts = false)
        {
            var query = _context.Carts.AsQueryable();
            
            if (includeProducts)
            {
                query = query.Include(c => c.Items).ThenInclude(i => i.Product);
            }
            else
            {
                query = query.Include(c => c.Items);
            }

            var cart = await query.FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { Id = Guid.NewGuid(), UserId = userId, UpdatedAt = DateTime.UtcNow };
                await _context.Carts.AddAsync(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        public async Task<CartResponseDTO> GetCartByUserIdAsync(Guid userId)
        {
            var cart = await _context.Carts
                .AsNoTracking()
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = await GetActiveCartAsync(userId, true);
            }

            return _mapper.Map<CartResponseDTO>(cart);
        }

        public async Task<CartResponseDTO> AddItemToCartAsync(Guid userId, CartItemRequestDTO dto)
        {
            var cart = await GetActiveCartAsync(userId);
            
            var product = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == dto.ProductId);
                
            if (product == null) throw new NotFoundException("Product not found");

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

            return await GetCartByUserIdAsync(userId);
        }

        public async Task<CartResponseDTO> UpdateItemQuantityAsync(Guid userId, Guid cartItemId, CartItemUpdateDTO dto)
        {
            var cart = await GetActiveCartAsync(userId);
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

        public async Task<CartResponseDTO> RemoveItemFromCartAsync(Guid userId, Guid cartItemId)
        {
            var cart = await GetActiveCartAsync(userId);
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
