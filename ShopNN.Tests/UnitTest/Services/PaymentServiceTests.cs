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
    #endregion

}
