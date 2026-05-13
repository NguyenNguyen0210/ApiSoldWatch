using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopNN.DTOs;
using ShopNN.Services.Interface;
using System.Security.Claims;
using ShopNN.Shared.Exeptions;
using ShopNN.Shared.Wrappers;
using ShopNN.Shared.Enums;

namespace ShopNN.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;

        public OrderController(IOrderService orderService, IPaymentService paymentService)
        {
            _orderService = orderService;
            _paymentService = paymentService;
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] OrderCreateRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.FailureResult("Invalid data", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var userId = GetUserId();
            if (userId == null)
                throw new UnauthorizedException("Session expired.");

            var orderResponse = await _orderService.CreateOrderAsync(userId.Value, request.PaymentMethod);

            if (request.PaymentMethod == PaymentMethod.VnPay)
            {
                orderResponse.PaymentUrl = await _paymentService.CreatePaymentUrlByOrderId(orderResponse.Id, HttpContext);
            }

            return Ok(ApiResponse<OrderResponseDTO>.SuccessResult(orderResponse, "Order created successfully."));
        }

        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized(ApiResponse<object>.FailureResult("Session expired."));

            var orders = await _orderService.GetMyOrdersAsync(userId.Value);
            return Ok(ApiResponse<List<OrderResponseDTO>>.SuccessResult(orders, "Orders retrieved successfully."));
        }

        [Authorize(Roles = RoleNames.Admin)]
        [HttpGet("admin/all")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _orderService.GetAllOrdersAsync();
            return Ok(ApiResponse<List<OrderResponseDTO>>.SuccessResult(result, "All orders retrieved successfully."));
        }

        [Authorize(Roles = RoleNames.Admin)]
        [HttpGet("admin/search")]
        public async Task<IActionResult> SearchOrders([FromQuery] OrderQueryDTO query)
        {
            var result = await _orderService.GetAllOrdersPagedAsync(query);
            return Ok(ApiResponse<PagedResult<OrderResponseDTO>>.SuccessResult(result, "Orders retrieved successfully."));
        }

        [Authorize(Roles = RoleNames.Admin)]
        [HttpPut("admin/{id}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] OrderStatus status)
        {
            var result = await _orderService.UpdateStatusAsync(id, status);
            return Ok(ApiResponse<OrderResponseDTO>.SuccessResult(result, "Order status updated successfully."));
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