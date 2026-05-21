using Microsoft.Extensions.Logging;
using ShopNN.Entities;
using ShopNN.Repositories.Interface;
using ShopNN.Services.Interface;
using ShopNN.Shared.Enums;
using ShopNN.Shared.Exceptions;
using ShopNN.Shared.Helper;

namespace ShopNN.Services.Implement
{
    public class PaymentService : IPaymentService
    {
        private readonly IConfiguration _configuration;
        private readonly IOrderRepository _orderRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            IConfiguration configuration,
            IOrderRepository orderRepository,
            IPaymentRepository paymentRepository,
            ICartRepository cartRepository,
            IProductRepository productRepository,
            ILogger<PaymentService> logger)
        {
            _configuration = configuration;
            _orderRepository = orderRepository;
            _paymentRepository = paymentRepository;
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _logger = logger;
        }

        public string CreatePaymentUrl(Order order, HttpContext context)
        {
            _logger.LogInformation("Creating VnPay payment URL for Order ID: {OrderId}, TotalAmount: {TotalAmount}", order.Id, order.TotalAmount);
            var vnpay = new VnPayHelper();
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
            vnpay.AddRequestData("vnp_IpAddr", context.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1");
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", "Payment for order: " + order.Id);
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", vnp_ReturnUrl!);
            vnpay.AddRequestData("vnp_TxnRef", order.Id.ToString());

            var paymentUrl = vnpay.CreateRequestUrl(vnp_Url!, vnp_HashSecret!);
            _logger.LogInformation("Successfully generated VnPay URL for Order ID: {OrderId}", order.Id);
            return paymentUrl;
        }

        public async Task<string> CreatePaymentUrlByOrderId(Guid orderId, HttpContext context)
        {
            _logger.LogInformation("Attempting to create payment URL for Order ID: {OrderId}", orderId);
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
            {
                _logger.LogWarning("Failed to create payment URL. Order ID: {OrderId} not found.", orderId);
                throw new NotFoundException($"Order {orderId} not found.");
            }

            return CreatePaymentUrl(order, context);
        }

        public async Task<bool> ProcessVnPayReturn(IQueryCollection collections)
        {
            var vnpay = new VnPayHelper();
            foreach (var (key, value) in collections)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                    vnpay.AddResponseData(key, value!);
            }

            var txnRef = vnpay.GetResponseData("vnp_TxnRef");
            if (string.IsNullOrEmpty(txnRef) || !Guid.TryParse(txnRef, out var orderId))
            {
                _logger.LogWarning("VnPay callback contains invalid or missing vnp_TxnRef: {TxnRef}", txnRef);
                return false;
            }

            var vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
            var vnp_TransactionNo = vnpay.GetResponseData("vnp_TransactionNo");
            var vnp_SecureHash = collections["vnp_SecureHash"];
            var hashSecret = _configuration["VnPay:HashSecret"];

            _logger.LogInformation("Received VnPay callback for Order ID: {OrderId}. ResponseCode: {ResponseCode}, TransactionNo: {TransactionNo}", orderId, vnp_ResponseCode, vnp_TransactionNo);

            if (!vnpay.ValidateSignature(vnp_SecureHash!, hashSecret!))
            {
                _logger.LogWarning("VnPay callback signature validation failed for Order ID: {OrderId}.", orderId);
                return false;
            }

            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
            {
                _logger.LogWarning("VnPay callback order lookup failed. Order ID: {OrderId} not found.", orderId);
                return false;
            }

            var payment = await _paymentRepository.GetByOrderIdAsync(orderId);
            bool isNewPayment = false;
            if (payment == null)
            {
                isNewPayment = true;
                payment = new Payment
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    Amount = order.TotalAmount,
                    CreatedAt = DateTime.UtcNow
                };
            }

            payment.TransactionId = vnp_TransactionNo;
            payment.PaymentDate = DateTime.UtcNow;

            if (vnp_ResponseCode == "00")
            {
                _logger.LogInformation("VnPay transaction marked as Success for Order ID: {OrderId}", orderId);
                payment.Status = "Success";
                order.PaymentStatus = PaymentStatus.Paid;
                order.Status = OrderStatus.Processing;

                // Clear the cart upon successful payment confirmation
                var cart = await _cartRepository.GetCartByUserIdAsync(order.UserId);
                if (cart != null)
                {
                    _logger.LogInformation("Payment successful. Clearing Cart ID: {CartId} for User ID: {UserId}", cart.Id, order.UserId);
                    await _cartRepository.ClearCartAsync(cart.Id);
                    await _cartRepository.SaveChangeAsync();
                }
            }
            else
            {
                _logger.LogWarning("VnPay transaction marked as Failed for Order ID: {OrderId} with ResponseCode: {ResponseCode}", orderId, vnp_ResponseCode);
                payment.Status = "Failed";
                order.PaymentStatus = PaymentStatus.Failed;
                order.Status = OrderStatus.Cancelled;

                // Restore stock for each product in the order
                if (order.Items != null)
                {
                    foreach (var item in order.Items)
                    {
                        var product = item.Product ?? await _productRepository.GetByIdAsync(item.ProductId);
                        if (product != null)
                        {
                            product.Stock += item.Quantity;
                            _logger.LogInformation("Payment failed/cancelled. Restored stock for Product ID: {ProductId}. Quantity: +{Qty}", item.ProductId, item.Quantity);
                        }
                    }
                }
                await _orderRepository.SaveChangesAsync();
            }

            if (isNewPayment)
            {
                await _paymentRepository.AddAsync(payment);
                _logger.LogInformation("Created new payment record ID: {PaymentId} for Order ID: {OrderId}", payment.Id, orderId);
            }
            else
            {
                _logger.LogInformation("Updated existing payment record ID: {PaymentId} for Order ID: {OrderId}", payment.Id, orderId);
            }

            await _paymentRepository.SaveChangesAsync();
            return vnp_ResponseCode == "00";
        }
    }
}
