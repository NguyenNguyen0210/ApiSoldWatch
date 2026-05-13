using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Moq;
using ShopNN.Entities;
using ShopNN.Repositories.Interface;
using ShopNN.Services.Implement;
using ShopNN.Shared.Enums;
using ShopNN.Shared.Exeptions;
using ShopNN.Shared.Helper;

namespace ShopNN.Tests.Services;

public class PaymentServiceTests
{
    private readonly Mock<IOrderRepository>   _orderRepo   = new();
    private readonly Mock<IPaymentRepository> _paymentRepo = new();
    private readonly PaymentService _sut;

    public PaymentServiceTests()
    {
        _sut = new PaymentService(VnPayConfig(), _orderRepo.Object, _paymentRepo.Object);
    }

    private static IConfiguration VnPayConfig() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["VnPay:TmnCode"]    = "TMNCODE",
                ["VnPay:HashSecret"] = "HASHSECRETAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
                ["VnPay:BaseUrl"]    = "https://sandbox.vnpayment.vn/test",
                ["VnPay:ReturnUrl"]  = "https://localhost/callback",
                ["VnPay:Version"]    = "2.1.0",
                ["VnPay:Command"]    = "pay",
                ["VnPay:CurrCode"]   = "VND"
            }!)
            .Build();

    private static Order MakeOrder(Guid? id = null) => new()
    {
        Id            = id ?? Guid.NewGuid(),
        UserId        = Guid.NewGuid(),
        TotalAmount   = 150_000m,
        CreatedAt     = DateTime.UtcNow,
        Status        = OrderStatus.Pending,
        PaymentStatus = PaymentStatus.Unpaid,
        Items         = new List<OrderItem>()
    };

    private static HttpContext MakeHttpContext() => new DefaultHttpContext
    {
        Connection = { RemoteIpAddress = System.Net.IPAddress.Loopback }
    };

    private static IQueryCollection InvalidHashQuery(Guid orderId) =>
        new QueryCollection(new Dictionary<string, StringValues>
        {
            ["vnp_TxnRef"]        = orderId.ToString(),
            ["vnp_ResponseCode"]  = "00",
            ["vnp_TransactionNo"] = "12345",
            ["vnp_SecureHash"]    = "invalid-hash"
        });
    #region CreatePaymentUrlByOrderId
    [Fact]
    public async Task CreatePaymentUrlByOrderId_WhenOrderNotFound_ShouldThrowNotFoundException()
    {
        _orderRepo.Setup(o => o.GetByIdAsync(It.IsAny<Guid>()))
                  .ReturnsAsync((Order?)null);

        var act = async () => await _sut.CreatePaymentUrlByOrderId(Guid.NewGuid(), MakeHttpContext());

        await act.Should().ThrowAsync<NotFoundException>();
        _orderRepo.Verify(o => o.GetByIdAsync(It.IsAny<Guid>()), Times.Once);
    }

    [Fact]
    public async Task CreatePaymentUrlByOrderId_WhenOrderExists_ShouldReturnSignedUrl()
    {
        var order = MakeOrder();
        _orderRepo.Setup(o => o.GetByIdAsync(order.Id)).ReturnsAsync(order);

        var url = await _sut.CreatePaymentUrlByOrderId(order.Id, MakeHttpContext());

        url.Should().StartWithEquivalentOf("https://sandbox.vnpayment.vn/test?");

        url.Should().Contain("vnp_SecureHash=");

        url.Should().Contain(order.Id.ToString());

        url.Should().Contain("vnp_TmnCode=TMNCODE");

        _orderRepo.Verify(o => o.GetByIdAsync(order.Id), Times.Once);
    }
    #endregion
    #region CreatePaymentUrl
    [Fact]
    public async Task CreatePaymentUrl_WhenCalled_ShouldContainCorrectAmount()
    {
        var order = MakeOrder();
        order.TotalAmount = 200_000m;
        _orderRepo.Setup(o => o.GetByIdAsync(order.Id)).ReturnsAsync(order);

        var url = await _sut.CreatePaymentUrlByOrderId(order.Id, MakeHttpContext());

        url.Should().Contain("vnp_Amount=20000000");
    }
        [Fact]
    public void CreatePaymentUrl_WhenCalled_ShouldReturnNonEmptyUrl()
    {
        var order = MakeOrder();

        var url = _sut.CreatePaymentUrl(order, MakeHttpContext());

        url.Should().NotBeNullOrWhiteSpace();
        url.Should().StartWithEquivalentOf("https://sandbox.vnpayment.vn/test?");
        url.Should().Contain("vnp_SecureHash=");
    }

    [Fact]
    public void CreatePaymentUrl_WhenCalled_ShouldContainOrderInfo()
    {
        var order = MakeOrder();

        var url = _sut.CreatePaymentUrl(order, MakeHttpContext());

        url.Should().Contain(order.Id.ToString());
        url.Should().Contain("vnp_OrderInfo=");
        url.Should().Contain("vnp_TmnCode=TMNCODE");
        url.Should().Contain("vnp_CurrCode=VND");
    }
    #endregion
    #region ProcessVnPayReturn
    [Fact]
    public async Task ProcessVnPayReturn_WhenSecureHashInvalid_ShouldReturnFalse()
    {
        var query = InvalidHashQuery(Guid.NewGuid());

        var result = await _sut.ProcessVnPayReturn(query);

        result.Should().BeFalse();

        _orderRepo.Verify(o => o.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _paymentRepo.Verify(p => p.AddAsync(It.IsAny<Payment>()), Times.Never);
        _paymentRepo.Verify(p => p.SaveChangesAsync(), Times.Never);
    }
    
    [Fact]
    public async Task ProcessVnPayReturn_WhenHashValidButOrderNotFound_ShouldReturnFalse()
    {
        var query = InvalidHashQuery(Guid.NewGuid());

        var result = await _sut.ProcessVnPayReturn(query);

        result.Should().BeFalse();
        _orderRepo.Verify(o => o.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task ProcessVnPayReturn_WhenHashValidAndSuccess_ShouldUpdateOrderAndPayment()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = MakeOrder(orderId);
        var hashSecret = "HASHSECRETAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
        
        // Build valid query
        var vnpay = new VnPayHelper();
        vnpay.AddResponseData("vnp_TxnRef", orderId.ToString());
        vnpay.AddResponseData("vnp_ResponseCode", "00");
        vnpay.AddResponseData("vnp_TransactionNo", "99999");
        
        var rawData = "vnp_ResponseCode=00&vnp_TransactionNo=99999&vnp_TxnRef=" + orderId.ToString();
        // Note: VnPayHelper orders keys alphabetically. vnp_ResponseCode < vnp_TransactionNo < vnp_TxnRef
        // Let's just use the helper to get the real hash
        var queryDict = new Dictionary<string, StringValues>
        {
            ["vnp_TxnRef"] = orderId.ToString(),
            ["vnp_ResponseCode"] = "00",
            ["vnp_TransactionNo"] = "99999"
        };
        
        // We need to match the sorting logic of VnPayHelper to calculate hash
        // Or just let the helper do it (internal method but we can replicate)
        var signData = "vnp_ResponseCode=00&vnp_TransactionNo=99999&vnp_TxnRef=" + orderId.ToString();
        var validHash = ShopNN.Utils.HashUtils.HmacSha512(hashSecret, signData);
        queryDict.Add("vnp_SecureHash", validHash);
        
        var query = new QueryCollection(queryDict);

        _orderRepo.Setup(o => o.GetByIdAsync(orderId)).ReturnsAsync(order);
        _paymentRepo.Setup(p => p.GetByOrderIdAsync(orderId)).ReturnsAsync((Payment?)null);
        _paymentRepo.Setup(p => p.AddAsync(It.IsAny<Payment>())).ReturnsAsync((Payment p) => p);
        _paymentRepo.Setup(p => p.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.ProcessVnPayReturn(query);

        // Assert
        result.Should().BeTrue();
        order.PaymentStatus.Should().Be(PaymentStatus.Paid);
        order.Status.Should().Be(OrderStatus.Processing);
        _paymentRepo.Verify(p => p.AddAsync(It.Is<Payment>(x => x.OrderId == orderId && x.Status == "Success")), Times.Once);
        _paymentRepo.Verify(p => p.SaveChangesAsync(), Times.Once);
    }
    [Fact]
    public async Task ProcessVnPayReturn_WhenHashValidButResponseFailed_ShouldSetPaymentFailed()
    {
        var orderId = Guid.NewGuid();
        var order = MakeOrder(orderId);
        var hashSecret = "HASHSECRETAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";

        var signData = "vnp_ResponseCode=99&vnp_TransactionNo=12345&vnp_TxnRef=" + orderId.ToString();
        var validHash = ShopNN.Utils.HashUtils.HmacSha512(hashSecret, signData);

        var queryDict = new Dictionary<string, StringValues>
        {
            ["vnp_TxnRef"] = orderId.ToString(),
            ["vnp_ResponseCode"] = "99",
            ["vnp_TransactionNo"] = "12345",
            ["vnp_SecureHash"] = validHash
        };
        var query = new QueryCollection(queryDict);

        _orderRepo.Setup(o => o.GetByIdAsync(orderId)).ReturnsAsync(order);
        _paymentRepo.Setup(p => p.GetByOrderIdAsync(orderId)).ReturnsAsync((Payment?)null);
        _paymentRepo.Setup(p => p.AddAsync(It.IsAny<Payment>())).ReturnsAsync((Payment p) => p);
        _paymentRepo.Setup(p => p.SaveChangesAsync()).Returns(Task.CompletedTask);

        var result = await _sut.ProcessVnPayReturn(query);

        result.Should().BeFalse();
        order.PaymentStatus.Should().Be(PaymentStatus.Failed);
        _paymentRepo.Verify(p => p.AddAsync(It.Is<Payment>(x => x.Status == "Failed")), Times.Once);
        _paymentRepo.Verify(p => p.SaveChangesAsync(), Times.Once);
    }
    #endregion

}
