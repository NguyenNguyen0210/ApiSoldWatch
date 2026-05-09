using Microsoft.AspNetCore.Mvc;
using ShopNN.Services.Interface;
using ShopNN.Shared.Wrappers;

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
                return Ok(ApiResponse<object>.SuccessResult("Payment successful"));
            }
            
            return BadRequest(ApiResponse<object>.FailureResult("Payment failed or invalid signature"));
        }
    }
}
