using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ShopNN.Entities;
using ShopNN.Services.Interface;
using ShopNN.Utils;

namespace ShopNN.Services.Implement
{
    public class PaymentService : IPaymentService
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public PaymentService(IConfiguration configuration, ApplicationDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        public string CreatePaymentUrl(Order order, HttpContext context)
        {
            var vnpay = new VnPayLibrary();
            var vnp_TmnCode = _configuration["VnPay:TmnCode"];
            var vnp_HashSecret = _configuration["VnPay:HashSecret"];
            var vnp_Url = _configuration["VnPay:BaseUrl"];
            var vnp_ReturnUrl = _configuration["VnPay:ReturnUrl"];

            vnpay.AddRequestData("vnp_Version", _configuration["VnPay:Version"] ?? "2.1.0");
            vnpay.AddRequestData("vnp_Command", _configuration["VnPay:Command"] ?? "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode!);
            vnpay.AddRequestData("vnp_Amount", ((long)(order.TotalAmount * 100)).ToString());
            vnpay.AddRequestData("vnp_CreateDate", order.CreatedAt.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", _configuration["VnPay:CurrCode"] ?? "VND");
            vnpay.AddRequestData("vnp_IpAddr", context.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1" ) ;
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", "Payment for order: " + order.Id);
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", vnp_ReturnUrl!);
            vnpay.AddRequestData("vnp_TxnRef", order.Id.ToString());

            return vnpay.CreateRequestUrl(vnp_Url!, vnp_HashSecret!);
        }

        public async Task<bool> ProcessVnPayReturn(IQueryCollection collections)
        {
            var vnpay = new VnPayLibrary();
            foreach (var (key, value) in collections)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(key, value!);
                }
            }

            var orderId = Guid.Parse(vnpay.GetResponseData("vnp_TxnRef"));
            var vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
            var vnp_TransactionNo = vnpay.GetResponseData("vnp_TransactionNo");
            var vnp_SecureHash = collections["vnp_SecureHash"];
            var hashSecret = _configuration["VnPay:HashSecret"];

            bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash!, hashSecret!);

            if (checkSignature)
            {
                var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
                if (order != null)
                {
                    var payment = await _context.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId);
                    if (payment == null)
                    {
                        payment = new Payment
                        {
                            Id = Guid.NewGuid(),
                            OrderId = orderId,
                            Amount = order.TotalAmount,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.Payments.Add(payment);
                    }

                    payment.TransactionId = vnp_TransactionNo;
                    payment.PaymentDate = DateTime.UtcNow;

                    if (vnp_ResponseCode == "00")
                    {
                        payment.Status = "Success";
                        order.PaymentStatus = PaymentStatus.Paid;
                        order.Status = OrderStatus.Processing;
                    }
                    else
                    {
                        payment.Status = "Failed";
                        order.PaymentStatus = PaymentStatus.Failed;
                    }

                    await _context.SaveChangesAsync();
                    return vnp_ResponseCode == "00";
                }
            }

            return false;
        }
    }
}
