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


        public async Task<CartResponseDTO> GetCartByUserIdAsync(Guid userId)
        {
            var cart = await _context.Carts
                .AsNoTracking()
                .Where(c => c.UserId == userId)
                .Select(c => new CartResponseDTO
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    Items = c.Items.Select(i => new CartItemResponseDTO
                    {
                        Id = i.Id,
                        ProductId = i.ProductId,
                        Quantity = i.Quantity,
                        ProductName = i.Product.Name,
                        ProductPrice = i.Product.Price,
                        ProductImageUrl = i.Product.ImageUrl
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (cart != null) return cart;

            var creatCart = new Cart()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.Carts.AddAsync(creatCart);
            await _context.SaveChangesAsync();
            return _mapper.Map<CartResponseDTO>(creatCart);
        }

        public async Task<CartResponseDTO> AddItemToCartAsync(Guid userId, CartItemRequestDTO dto)
        {
            if (dto.Quantity <= 0) throw new BadRequestException("Quantity must be greater than 0");

            var product = await _context.Products
                .AsNoTracking()
                .Where(p => p.Id == dto.ProductId)
                .Select(p => new { p.Id, p.Stock })
                .FirstOrDefaultAsync();

            if (product == null)
                throw new NotFoundException("Product not found");

            if (product.Stock < dto.Quantity)
                throw new BadRequestException("Not enough stock");

            var cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId);

            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(i => i.CartId == cart.Id && i.ProductId == dto.ProductId);

            if (existingItem != null)
            {
                existingItem.Quantity += dto.Quantity;
            }
            else
            {
                await _context.CartItems.AddAsync(new CartItem
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
            var item = await _context.CartItems
                .Include(i => i.Cart)
                .FirstOrDefaultAsync(i => i.Id == cartItemId && i.Cart.UserId == userId);

            if (item == null)
                throw new NotFoundException("Cart item not found");

            if (dto.Quantity <= 0)
            {
                _context.CartItems.Remove(item);
            }
            else
            {
                item.Quantity = dto.Quantity;
            }

            item.Cart.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return await GetCartByUserIdAsync(userId);
        }

        public async Task<CartResponseDTO> RemoveItemFromCartAsync(Guid userId, Guid cartItemId)
        {
            var item = await _context.CartItems
                .Include(i => i.Cart)
                .FirstOrDefaultAsync(i => i.Id == cartItemId && i.Cart.UserId == userId);
            if(item == null) throw new NotFoundException("Item not found");

            item.Cart.UpdatedAt = DateTime.UtcNow;
            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();
            

            return await GetCartByUserIdAsync(userId);
        }

        public async Task<bool> ClearCartAsync(Guid userId)
        {
            var cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null) return true;

            var items = await _context.CartItems
                .Where(i => i.CartId == cart.Id)
                .ToListAsync();

            if (items.Any())
            {
                _context.CartItems.RemoveRange(items);
                cart.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return true;
        }
    }
}