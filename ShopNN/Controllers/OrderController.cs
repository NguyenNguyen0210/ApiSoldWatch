using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopNN.DTOs;
using ShopNN.Services.Interface;
using System.Security.Claims;

namespace ShopNN.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // 🔥 yêu cầu login cho toàn bộ controller
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // =========================
        // CREATE ORDER
        // =========================
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] List<OrderItemDTO> items)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserId();
            if (userId == null)
                return Unauthorized("Invalid token");

            var result = await _orderService.CreateOrderAsync(userId.Value, items);

            return Ok(result);
        }

        // =========================
        // MY ORDERS
        // =========================
        [HttpGet]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized("Invalid token");

            var orders = await _orderService.GetMyOrdersAsync(userId.Value);

            return Ok(orders);
        }

        // =========================
        // ADMIN
        // =========================
        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _orderService.GetAllOrdersAsync();
            return Ok(result);
        }

        // =========================
        // HELPER
        // =========================
        private Guid? GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (Guid.TryParse(userIdClaim, out var userId))
                return userId;

            return null;
        }
    }
}