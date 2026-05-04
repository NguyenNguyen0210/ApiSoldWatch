using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopNN.DTOs;
using ShopNN.Services.Interface;
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
            try
            {
                var cart = await _cartService.GetCartByUserIdAsync(GetUserId());
                return Ok(ApiResponse<CartDTO>.SuccessResult(cart, "Cart retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.FailureResult(ex.Message));
            }
        }

        [HttpPost("items")]
        public async Task<IActionResult> AddItem([FromBody] AddToCartDTO dto)
        {
            try
            {
                var cart = await _cartService.AddItemToCartAsync(GetUserId(), dto);
                return Ok(ApiResponse<CartDTO>.SuccessResult(cart, "Item added to cart successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.FailureResult(ex.Message));
            }
        }

        [HttpPut("items/{itemId}")]
        public async Task<IActionResult> UpdateQuantity(Guid itemId, [FromBody] UpdateCartItemDTO dto)
        {
            try
            {
                var cart = await _cartService.UpdateItemQuantityAsync(GetUserId(), itemId, dto);
                return Ok(ApiResponse<CartDTO>.SuccessResult(cart, "Cart item quantity updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.FailureResult(ex.Message));
            }
        }

        [HttpDelete("items/{itemId}")]
        public async Task<IActionResult> RemoveItem(Guid itemId)
        {
            try
            {
                var cart = await _cartService.RemoveItemFromCartAsync(GetUserId(), itemId);
                return Ok(ApiResponse<CartDTO>.SuccessResult(cart, "Item removed from cart successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.FailureResult(ex.Message));
            }
        }

        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            try
            {
                await _cartService.ClearCartAsync(GetUserId());
                return Ok(ApiResponse.SuccessResult("Cart cleared successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.FailureResult(ex.Message));
            }
        }
    }
}
