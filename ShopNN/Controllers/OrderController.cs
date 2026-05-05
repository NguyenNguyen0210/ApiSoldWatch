using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopNN.DTOs;
using ShopNN.Services.Interface;
using System.Security.Claims;
using ShopNN.Exceptions;
using ShopNN.Entities;

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

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderCreateRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse.FailureResult("Invalid data", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var userId = GetUserId();
            if (userId == null)
                throw new UnauthorizedException("Invalid token");

            var result = await _orderService.CreateOrderAsync(userId.Value, request.PaymentMethod);

            if (request.PaymentMethod == PaymentMethod.VnPay)
            {
                var order = new Order { Id = result.Id, TotalAmount = result.TotalAmount, CreatedAt = result.CreatedAt };
                result.PaymentUrl = _paymentService.CreatePaymentUrl(order, HttpContext);
            }

            return Ok(ApiResponse<OrderResponseDTO>.SuccessResult(result, "Order created successfully"));
        }

        [HttpGet]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized(ApiResponse.FailureResult("Invalid token"));

            var orders = await _orderService.GetMyOrdersAsync(userId.Value);

            return Ok(ApiResponse<List<OrderResponseDTO>>.SuccessResult(orders, "Orders retrieved successfully"));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _orderService.GetAllOrdersAsync();
            return Ok(ApiResponse<List<OrderResponseDTO>>.SuccessResult(result, "All orders retrieved successfully"));
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] OrderStatus status)
        {
            var result = await _orderService.UpdateStatusAsync(id, status);
            return Ok(ApiResponse<OrderResponseDTO>.SuccessResult(result, "Order status updated successfully"));
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