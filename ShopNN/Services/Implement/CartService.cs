using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShopNN.DTOs.Account;
using ShopNN.DTOs.Product;
using ShopNN.DTOs.Category;
using ShopNN.DTOs.Cart;
using ShopNN.DTOs.Order;
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
        private readonly ILogger<CartService> _logger;

        public CartService(ICartRepository cartRepository, IMapper mapper, IProductRepository productRepository, ILogger<CartService> logger)
        {
            _productRepository = productRepository;
            _cartRepository = cartRepository;
            _mapper = mapper;
            _logger = logger;
        }
        public async Task<CartResponseDTO> GetCartByIdAsync(Guid Id)
        {
            var cart = await _cartRepository.GetByIdAsync(Id);
            if (cart == null) throw new NotFoundException($"Cart Id: {Id} Not Found");
            return _mapper.Map<CartResponseDTO>( cart );

        }

        public async Task<CartResponseDTO> GetCartByUserIdAsync(Guid userId)
        {
            _logger.LogInformation("Retrieving cart for User ID: {UserId}", userId);
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
            _logger.LogInformation("No cart found for User ID: {UserId}. Created new Cart ID: {CartId}", userId, creatCart.Id);
            return _mapper.Map<CartResponseDTO>(creatCart);
        }

        public async Task<CartResponseDTO> AddItemToCartAsync(Guid userId, CartItemRequestDTO dto)
        {
            if (dto.Quantity <= 0) throw new BadRequestException("Quantity must be greater than 0");

            _logger.LogInformation("User ID: {UserId} requested adding Product ID: {ProductId} with Quantity: {Quantity} to cart", userId, dto.ProductId, dto.Quantity);
            var cart = await _cartRepository.GetCartForUpdateAsync(userId);

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
                _logger.LogInformation("Created new Cart ID: {CartId} for User ID: {UserId}", cart.Id, userId);
            }

            var product = await _productRepository.GetByIdAsync(dto.ProductId);

            if (product == null)
            {
                _logger.LogWarning("Failed to add product. Product ID: {ProductId} not found", dto.ProductId);
                throw new NotFoundException("Product not found");
            }

            if (product.Stock < dto.Quantity)
            {
                _logger.LogWarning("Failed to add Product ID: {ProductId} to Cart. Requested: {Qty}, Available Stock: {Stock}", dto.ProductId, dto.Quantity, product.Stock);
                throw new BadRequestException("Not enough stock");
            }

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == dto.ProductId);

            if (existingItem != null)
            {
                var newQuantity = existingItem.Quantity + dto.Quantity;
                if (newQuantity > product.Stock)
                {
                    _logger.LogWarning("Cannot increase quantity of Product ID: {ProductId}. In Cart: {CartQty}, Requested addition: {AddQty}, Available Stock: {Stock}", dto.ProductId, existingItem.Quantity, dto.Quantity, product.Stock);
                    throw new BadRequestException(
                        $"Not enough stock. Available: {product.Stock}, In cart: {existingItem.Quantity}");
                }

                existingItem.Quantity = newQuantity;
                _logger.LogInformation("Increased product ID: {ProductId} quantity in Cart ID: {CartId} to {NewQty}", dto.ProductId, cart.Id, newQuantity);
            }
            else
            {
                var newItem = new CartItem()
                {
                    CartId = cart.Id,
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity
                };

                cart.Items.Add(newItem);
                _logger.LogInformation("Added new product ID: {ProductId} with Quantity: {Quantity} to Cart ID: {CartId}", dto.ProductId, dto.Quantity, cart.Id);
            }

            cart.UpdatedAt = DateTime.UtcNow;
            await _cartRepository.SaveChangeAsync();

            var updateCart = await _cartRepository.GetCartByUserIdAsync(userId);
            return _mapper.Map<CartResponseDTO>(updateCart);
        }

        public async Task<CartResponseDTO> UpdateItemQuantityAsync(Guid userId, Guid cartItemId, CartItemUpdateDTO dto)
        {
            if (dto.Quantity <= 0) throw new BadRequestException("Quantity must be at least 1");

            _logger.LogInformation("User ID: {UserId} requested updating CartItem ID: {CartItemId} quantity to {Quantity}", userId, cartItemId, dto.Quantity);
            var cart = await _cartRepository.GetCartForUpdateAsync(userId) ?? throw new NotFoundException("Cart Not Found");
            var item = cart.Items.FirstOrDefault(x => x.Id == cartItemId) ?? throw new NotFoundException("Cart item not found");

            var product = await _productRepository.GetByIdAsync(item.ProductId) ?? throw new NotFoundException("Product not found");

            if (product.Stock < dto.Quantity)
            {
                _logger.LogWarning("Failed to update CartItem ID: {CartItemId} quantity. Requested: {Qty}, Available Stock: {Stock}", cartItemId, dto.Quantity, product.Stock);
                throw new BadRequestException("Not enough stock");
            }

            item.Quantity = dto.Quantity;
            cart.UpdatedAt = DateTime.UtcNow;
            await _cartRepository.SaveChangeAsync();
            _logger.LogInformation("Updated CartItem ID: {CartItemId} quantity to {Quantity} in Cart ID: {CartId}", cartItemId, dto.Quantity, cart.Id);

            var updateCart = await _cartRepository.GetCartByUserIdAsync(userId);
            return _mapper.Map<CartResponseDTO>(updateCart);
        }

        public async Task<CartResponseDTO> RemoveItemFromCartAsync(Guid userId, Guid cartItemId)
        {
            _logger.LogInformation("User ID: {UserId} requested removing CartItem ID: {CartItemId}", userId, cartItemId);
            var cart = await _cartRepository.GetCartForUpdateAsync(userId) ?? throw new NotFoundException("Cart Not Found");
            var item = cart.Items.FirstOrDefault(x => x.Id == cartItemId) ?? throw new NotFoundException("Cart item not found");

            await _cartRepository.DeleteItemAsync(cartItemId);

            cart.UpdatedAt = DateTime.UtcNow;
            await _cartRepository.SaveChangeAsync();
            _logger.LogInformation("Removed CartItem ID: {CartItemId} from Cart ID: {CartId}", cartItemId, cart.Id);

            var updatedCart = await _cartRepository.GetCartByUserIdAsync(userId);
            return _mapper.Map<CartResponseDTO>(updatedCart);
        }

        public async Task ClearCartAsync(Guid userId)
        {
            _logger.LogInformation("User ID: {UserId} requested clearing cart", userId);
            var cart = await _cartRepository.GetCartForUpdateAsync(userId)
                ?? throw new NotFoundException("Cart not found");

            if (!cart.Items.Any())
                throw new BadRequestException("Cart is already empty");

            await _cartRepository.ClearCartAsync(cart.Id);

            cart.UpdatedAt = DateTime.UtcNow;
            await _cartRepository.SaveChangeAsync();
            _logger.LogInformation("Cleared all items from Cart ID: {CartId}", cart.Id);
        }
    }
}
