using Microsoft.AspNetCore.Mvc;
using ShopNN.Services.Interface;


namespace ShopNN.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IConfiguration _configuration;

        public PaymentController(IPaymentService paymentService,IConfiguration configuration)
        {
            _paymentService = paymentService;
            _configuration = configuration;
        }

        [HttpGet("vnpay-return")]
        public async Task<IActionResult> VnPayReturn()
        {
            var result = await _paymentService.ProcessVnPayReturn(Request.Query);
            var orderId = Request.Query["vnp_TxnRef"].ToString();
            var baseUrl = _configuration["BaseUrlFE"];
            
            if (result)
            {
                return Redirect($"{baseUrl}/orders?payment=success&orderId={orderId}");
            }
            
            return Redirect($"{baseUrl}/orders?payment=fail&orderId={orderId}");
        }
    }
}
