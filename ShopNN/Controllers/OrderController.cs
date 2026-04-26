using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShopNN.DTOs;
using ShopNN.Services.Interface;

namespace ShopNN.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }
        [HttpPost("Order/{userId}")]
        public async Task<IActionResult> CreateOrder([FromRoute] Guid userId,[FromBody] List<OrderItemDTO> items)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _orderService.CreateOrderAsync(userId,items);
            return Ok(result);


        }

        [HttpGet("order/{userId}")]
        public async Task<IActionResult> GetMyOrders([FromRoute ] Guid userId)
        {
            var order = await _orderService.GetMyOrdersAsync(userId);
            return Ok(order);
        }
        [Authorize(Roles=  "Admin")]
        [HttpGet("order")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _orderService.GetAllOrdersAsync();
            return Ok(result);
        }
    }
}
