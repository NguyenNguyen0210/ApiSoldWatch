using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ShopNN.DTOs;
using ShopNN.Entities;
using ShopNN.Repositories.Interface;
using ShopNN.Services.Interface;
using ShopNN.Shared.Exceptions;

namespace ShopNN.Services.Implement
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public CartService(ICartRepository cartRepository ,IMapper mapper, IProductRepository productRepository)
        {
            _productRepository = productRepository;
            _cartRepository = cartRepository;
            _mapper = mapper;
        }
        public async Task<CartResponseDTO> GetCartByIdAsync(Guid Id)
        {
            var cart = await _cartRepository.GetByIdAsync(Id);
            if (cart == null) throw new NotFoundException($"Cart Id: {Id} Not Found");
            return _mapper.Map<CartResponseDTO>( cart );

        }

        public async Task<CartResponseDTO> GetCartByUserIdAsync(Guid userId)
        {
            var cart = await _cartRepository.GetCartByUserIdAsync(userId);

            if (cart != null) return _mapper.Map<CartResponseDTO>(cart);

            var creatCart = new Cart()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                UpdatedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
            };

            await _cartRepository.AddAsync(creatCart);
            return _mapper.Map<CartResponseDTO>(creatCart);
        }

        public async Task<CartResponseDTO> AddItemToCartAsync(Guid userId, CartItemRequestDTO dto)
        {
            if (dto.Quantity <= 0) throw new BadRequestException("Quantity must be greater than 0");

            var product = await _productRepository.GetByIdAsync(dto.ProductId);

            if (product == null)
                throw new NotFoundException("Product not found");

            if (product.Stock < dto.Quantity)
                throw new BadRequestException("Not enough stock");

            var cart = await _cartRepository.GetCartByUserIdAsync(userId) ;

            if (cart == null)
            {
                cart = new Cart
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _cartRepository.AddAsync(cart);
            }

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == dto.ProductId);

            if (existingItem != null)
            {
                var newQuantity = existingItem.Quantity + dto.Quantity;
                if (newQuantity > product.Stock)
                    throw new BadRequestException(
                        $"Not enough stock. Available: {product.Stock}, In cart: {existingItem.Quantity}");

                existingItem.Quantity = newQuantity;
            }
            else
            {
                var newItem = new CartItem()
                {
                    Id = Guid.NewGuid(),
                    CartId = cart.Id,
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity

                };

                cart.UpdatedAt = DateTime.UtcNow;
                
                cart.Items.Add(newItem);
            }
            await _cartRepository.SaveChangeAsync();

            var updateCart = await _cartRepository.GetCartByUserIdAsync(userId);
            return _mapper.Map<CartResponseDTO>(updateCart);
        }

        public async Task<CartResponseDTO> UpdateItemQuantityAsync(Guid userId, Guid cartItemId, CartItemUpdateDTO dto)
        {
            if (dto.Quantity <= 0) throw new BadRequestException("Quantity must be at least 1");

            var cart = await _cartRepository.GetCartByUserIdAsync(userId) ?? throw new NotFoundException("Cart Not Found");
            var item = cart.Items.FirstOrDefault(x => x.Id == cartItemId) ?? throw new NotFoundException("Cart item not found");

            var product = item.Product ?? throw new NotFoundException("Product not found");

            if (product.Stock < dto.Quantity)
                throw new BadRequestException("Not enough stock");

            item.Quantity = dto.Quantity;
            

            item.Cart.UpdatedAt = DateTime.UtcNow;
            await _cartRepository.SaveChangeAsync();
            var updateCart = await _cartRepository.GetCartByUserIdAsync(userId);
            return _mapper.Map<CartResponseDTO>(updateCart);
        }

        public async Task<CartResponseDTO> RemoveItemFromCartAsync(Guid userId, Guid cartItemId)
        {
            var cart = await _cartRepository.GetCartByUserIdAsync(userId) ?? throw new NotFoundException("Cart Not Found");
            var item = cart.Items.FirstOrDefault(x => x.Id == cartItemId) ?? throw new NotFoundException("Cart item not found");
            await _cartRepository.DeleteItemAsync(cartItemId);
            cart.UpdatedAt = DateTime.UtcNow;
            await _cartRepository.SaveChangeAsync();

            var updatedCart = await _cartRepository.GetCartByUserIdAsync(userId);
            return _mapper.Map<CartResponseDTO>(updatedCart);
        }

        public async Task ClearCartAsync(Guid userId)
        {
            var cart = await _cartRepository.GetCartByUserIdAsync(userId)
                ?? throw new NotFoundException("Cart not found");

            if (!cart.Items.Any())
                throw new BadRequestException("Cart is already empty");

            await _cartRepository.ClearCartAsync(cart.Id);

            cart.UpdatedAt = DateTime.UtcNow;
            await _cartRepository.SaveChangeAsync();
        }
    }
}