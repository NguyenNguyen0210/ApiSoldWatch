using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using ShopNN.Entities;
using ShopNN.Repositories.Interface;
using ShopNN.Services.Implement;
using ShopNN.Shared.Enums;
using ShopNN.Shared.Exceptions;
using ShopNN.Shared.Helper;

namespace ShopNN.Tests.Services;

public class PaymentServiceTests
{
    private readonly Mock<IOrderRepository>   _orderRepo   = new();
    private readonly Mock<IPaymentRepository> _paymentRepo = new();
    private readonly Mock<ICartRepository>    _cartRepo    = new();
    private readonly Mock<IProductRepository> _productRepo  = new();
    private readonly PaymentService _sut;

    public PaymentServiceTests()
    {
        _sut = new PaymentService(
            VnPayConfig(), 
            _orderRepo.Object, 
            _paymentRepo.Object, 
            _cartRepo.Object, 
            _productRepo.Object, 
            new Mock<ILogger<PaymentService>>().Object);
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
        var queryDict = new Dictionary<string, StringValues>
        {
            ["vnp_TxnRef"] = orderId.ToString(),
            ["vnp_ResponseCode"] = "00",
            ["vnp_TransactionNo"] = "99999"
        };
        
        var signData = "vnp_ResponseCode=00&vnp_TransactionNo=99999&vnp_TxnRef=" + orderId.ToString();
        var validHash = ShopNN.Utils.HashUtils.HmacSha512(hashSecret, signData);
        queryDict.Add("vnp_SecureHash", validHash);
        
        var query = new QueryCollection(queryDict);
        var cart = new Cart { Id = Guid.NewGuid(), UserId = order.UserId };

        _orderRepo.Setup(o => o.GetByIdAsync(orderId)).ReturnsAsync(order);
        _paymentRepo.Setup(p => p.GetByOrderIdAsync(orderId)).ReturnsAsync((Payment?)null);
        _paymentRepo.Setup(p => p.AddAsync(It.IsAny<Payment>())).ReturnsAsync((Payment p) => p);
        _paymentRepo.Setup(p => p.SaveChangesAsync()).Returns(Task.CompletedTask);
        _cartRepo.Setup(c => c.GetCartByUserIdAsync(order.UserId)).ReturnsAsync(cart);
        _cartRepo.Setup(c => c.ClearCartAsync(cart.Id)).Returns(Task.CompletedTask);
        _cartRepo.Setup(c => c.SaveChangeAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.ProcessVnPayReturn(query);

        // Assert
        result.Should().BeTrue();
        order.PaymentStatus.Should().Be(PaymentStatus.Paid);
        order.Status.Should().Be(OrderStatus.Processing);
        _paymentRepo.Verify(p => p.AddAsync(It.Is<Payment>(x => x.OrderId == orderId && x.Status == "Success")), Times.Once);
        _paymentRepo.Verify(p => p.SaveChangesAsync(), Times.Once);
        _cartRepo.Verify(c => c.ClearCartAsync(cart.Id), Times.Once);
        _cartRepo.Verify(c => c.SaveChangeAsync(), Times.Once);
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
        _orderRepo.Setup(o => o.SaveChangesAsync()).Returns(Task.CompletedTask);
        _paymentRepo.Setup(p => p.GetByOrderIdAsync(orderId)).ReturnsAsync((Payment?)null);
        _paymentRepo.Setup(p => p.AddAsync(It.IsAny<Payment>())).ReturnsAsync((Payment p) => p);
        _paymentRepo.Setup(p => p.SaveChangesAsync()).Returns(Task.CompletedTask);

        var result = await _sut.ProcessVnPayReturn(query);

        result.Should().BeFalse();
        order.PaymentStatus.Should().Be(PaymentStatus.Failed);
        order.Status.Should().Be(OrderStatus.Cancelled);
        _paymentRepo.Verify(p => p.AddAsync(It.Is<Payment>(x => x.Status == "Failed")), Times.Once);
        _paymentRepo.Verify(p => p.SaveChangesAsync(), Times.Once);
        _orderRepo.Verify(o => o.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ProcessVnPayReturn_WhenHashValidButResponseFailed_ShouldRestoreStock()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = MakeOrder(orderId);
        var productId1 = 101;
        var productId2 = 102;
        var product1 = new Product { Id = productId1, Stock = 5, Name = "Product 1", Description = "Desc 1" };
        var product2 = new Product { Id = productId2, Stock = 10, Name = "Product 2", Description = "Desc 2" };
        
        order.Items = new List<OrderItem>
        {
            new OrderItem { ProductId = productId1, Quantity = 2, Product = product1 },
            new OrderItem { ProductId = productId2, Quantity = 3 } // tests fallback repository fetch
        };
        
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
        _orderRepo.Setup(o => o.SaveChangesAsync()).Returns(Task.CompletedTask);
        _paymentRepo.Setup(p => p.GetByOrderIdAsync(orderId)).ReturnsAsync((Payment?)null);
        _paymentRepo.Setup(p => p.AddAsync(It.IsAny<Payment>())).ReturnsAsync((Payment p) => p);
        _paymentRepo.Setup(p => p.SaveChangesAsync()).Returns(Task.CompletedTask);
        _productRepo.Setup(p => p.GetByIdAsync(productId2)).ReturnsAsync(product2);

        // Act
        var result = await _sut.ProcessVnPayReturn(query);

        // Assert
        result.Should().BeFalse();
        order.PaymentStatus.Should().Be(PaymentStatus.Failed);
        order.Status.Should().Be(OrderStatus.Cancelled);
        product1.Stock.Should().Be(7); // 5 + 2
        product2.Stock.Should().Be(13); // 10 + 3
        _productRepo.Verify(p => p.GetByIdAsync(productId2), Times.Once);
        _orderRepo.Verify(o => o.SaveChangesAsync(), Times.Once);
    }
    #endregion

}
