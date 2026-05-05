using Microsoft.AspNetCore.Mvc;
using ShopNN.DTOs;
using ShopNN.Services.Interface;

namespace ShopNN.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet("vnpay-return")]
        public async Task<IActionResult> VnPayReturn()
        {
            var result = await _paymentService.ProcessVnPayReturn(Request.Query);
            
            if (result)
            {
                // Trong thực tế, bạn có thể redirect về trang Frontend thành công
                return Ok(ApiResponse.SuccessResult("Payment successful"));
            }
            
            return BadRequest(ApiResponse.FailureResult("Payment failed or invalid signature"));
        }
    }
}
