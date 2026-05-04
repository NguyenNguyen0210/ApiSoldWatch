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
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] List<OrderItemDTO> items)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse.FailureResult("Invalid data", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var userId = GetUserId();
            if (userId == null)
                return Unauthorized(ApiResponse.FailureResult("Invalid token"));

            try
            {
                var result = await _orderService.CreateOrderAsync(userId.Value, items);
                return Ok(ApiResponse<OrderDTO>.SuccessResult(result, "Order created successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.FailureResult(ex.Message));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized(ApiResponse.FailureResult("Invalid token"));

            var orders = await _orderService.GetMyOrdersAsync(userId.Value);

            return Ok(ApiResponse<List<OrderDTO>>.SuccessResult(orders, "Orders retrieved successfully"));
        }


        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _orderService.GetAllOrdersAsync();
            return Ok(ApiResponse<List<OrderDTO>>.SuccessResult(result, "All orders retrieved successfully"));
        }


        private Guid? GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (Guid.TryParse(userIdClaim, out var userId))
                return userId;

            return null;
        }
    }
}