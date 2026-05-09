using ShopNN.Entities;
using ShopNN.Repositories.Interface;
using ShopNN.Services.Interface;
using ShopNN.Shared.Enums;
using ShopNN.Shared.Exeptions;
using ShopNN.Shared.Helper;

public class PaymentService : IPaymentService
{
    private readonly IConfiguration _configuration;
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentRepository _paymentRepository;

    public PaymentService(
        IConfiguration configuration,
        IOrderRepository orderRepository,
        IPaymentRepository paymentRepository)
    {
        _configuration = configuration;
        _orderRepository = orderRepository;
        _paymentRepository = paymentRepository;
    }

    public string CreatePaymentUrl(Order order, HttpContext context)
    {
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

        return vnpay.CreateRequestUrl(vnp_Url!, vnp_HashSecret!);
    }

    public async Task<string> CreatePaymentUrlByOrderId(Guid orderId, HttpContext context)
    {
        var order = await _orderRepository.GetByIdAsync(orderId)
            ?? throw new NotFoundException($"Order {orderId} not found.");

        return CreatePaymentUrl(order, context);
    }

    public async Task<bool> ProcessVnPayReturn(IQueryCollection collections)
    {
        // 1. Parse response data
        var vnpay = new VnPayHelper();
        foreach (var (key, value) in collections)
        {
            if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                vnpay.AddResponseData(key, value!);
        }

        var orderId = Guid.Parse(vnpay.GetResponseData("vnp_TxnRef"));
        var vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
        var vnp_TransactionNo = vnpay.GetResponseData("vnp_TransactionNo");
        var vnp_SecureHash = collections["vnp_SecureHash"];
        var hashSecret = _configuration["VnPay:HashSecret"];

        // 2. Validate signature
        if (!vnpay.ValidateSignature(vnp_SecureHash!, hashSecret!))
            return false;

        // 3. Lấy order
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) return false;

        // 4. Upsert payment
        var payment = await _paymentRepository.GetByOrderIdAsync(orderId);
        if (payment == null)
        {
            payment = new Payment
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                Amount = order.TotalAmount,
                CreatedAt = DateTime.UtcNow
            };
            await _paymentRepository.AddAsync(payment);
        }

        payment.TransactionId = vnp_TransactionNo;
        payment.PaymentDate = DateTime.UtcNow;

        // 5. Cập nhật trạng thái
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

        // 6. SaveChanges 1 lần — EF track cả payment lẫn order
        await _paymentRepository.SaveChangesAsync();

        return vnp_ResponseCode == "00";
    }
}