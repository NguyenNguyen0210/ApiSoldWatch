using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopNN.DTOs;
using ShopNN.Services.Interface;
using ShopNN.Shared.Wrappers;
using System.Security.Claims;

namespace ShopNN.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        private Guid GetUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            {
                throw new UnauthorizedAccessException("User not found or invalid token.");
            }
            return userId;
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var cart = await _cartService.GetCartByUserIdAsync(GetUserId());
            return Ok(ApiResponse<CartResponseDTO>.SuccessResult(cart, "Cart retrieved successfully"));
        }

        [HttpPost("items")]
        public async Task<IActionResult> AddItem([FromBody] CartItemRequestDTO dto)
        {
            var cart = await _cartService.AddItemToCartAsync(GetUserId(), dto);
            return Ok(ApiResponse<CartResponseDTO>.SuccessResult(cart, "Item added to cart successfully"));
        }

        [HttpPut("items/{itemId}")]
        public async Task<IActionResult> UpdateQuantity(Guid itemId, [FromBody] CartItemUpdateDTO dto)
        {
            var cart = await _cartService.UpdateItemQuantityAsync(GetUserId(), itemId, dto);
            return Ok(ApiResponse<CartResponseDTO>.SuccessResult(cart, "Cart item quantity updated successfully"));
        }

        [HttpDelete("items/{itemId}")]
        public async Task<IActionResult> RemoveItem(Guid itemId)
        {
            var cart = await _cartService.RemoveItemFromCartAsync(GetUserId(), itemId);
            return Ok(ApiResponse<CartResponseDTO>.SuccessResult(cart, "Item removed from cart successfully"));
        }

        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            await _cartService.ClearCartAsync(GetUserId());
            return Ok(ApiResponse<object>.SuccessResult("Cart cleared successfully"));
        }
    }
}
