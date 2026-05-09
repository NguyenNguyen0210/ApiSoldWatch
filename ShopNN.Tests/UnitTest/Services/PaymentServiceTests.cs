using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Moq;
using ShopNN.Repositories.Interface;
using ShopNN.Shared.Exeptions;

namespace ShopNN.Tests.Services;

public class PaymentServiceTests
{
    private static IConfiguration VnPayConfiguration()
    {
        return new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["VnPay:TmnCode"] = "TMNCODE",
            ["VnPay:HashSecret"] = "HASHSECRETAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
            ["VnPay:BaseUrl"] = "https://sandbox.vnpayment.vn/test",
            ["VnPay:ReturnUrl"] = "https://localhost/callback",
            ["VnPay:Version"] = "2.1.0",
            ["VnPay:Command"] = "pay",
            ["VnPay:CurrCode"] = "VND"
        }!).Build();
    }

    private static PaymentService CreateSut(Mock<IOrderRepository>? orderRepo = null, Mock<IPaymentRepository>? paymentRepo = null)
    {
        var orm = orderRepo ?? new Mock<IOrderRepository>();
        var prm = paymentRepo ?? new Mock<IPaymentRepository>();
        return new PaymentService(VnPayConfiguration(), orm.Object, prm.Object);
    }

    [Fact]
    public async Task CreatePaymentUrlByOrderId_WhenMissing_ShouldThrow()
    {
        var orderRepo = new Mock<IOrderRepository>();
        orderRepo.Setup(o => o.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((global::Order?)null);
        var sut = CreateSut(orderRepo);

        var http = new DefaultHttpContext();

        var act = async () => await sut.CreatePaymentUrlByOrderId(Guid.NewGuid(), http);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreatePaymentUrlByOrderId_WhenExists_ShouldReturnSignedUrl()
    {
        var id = Guid.NewGuid();
        var order = new global::Order { Id = id, CreatedAt = DateTime.UtcNow, TotalAmount = 150000m };
        var orderRepo = new Mock<IOrderRepository>();
        orderRepo.Setup(o => o.GetByIdAsync(id)).ReturnsAsync(order);

        var sut = CreateSut(orderRepo);
        var http = new DefaultHttpContext { Connection = { RemoteIpAddress = System.Net.IPAddress.Loopback } };

        var url = await sut.CreatePaymentUrlByOrderId(id, http);

        url.Should().StartWithEquivalentOf("https://sandbox.vnpayment.vn/test?");
        url.Should().Contain("vnp_SecureHash=");
        url.Should().Contain(id.ToString());
    }

    [Fact]
    public async Task ProcessVnPayReturn_WhenSecureHashMismatch_ShouldReturnFalseWithoutLoadingOrder()
    {
        var orderId = Guid.NewGuid();
        var orderRepo = new Mock<IOrderRepository>();
        var paymentRepo = new Mock<IPaymentRepository>();

        var query = new QueryCollection(new Dictionary<string, StringValues>
        {
            ["vnp_TxnRef"] = orderId.ToString(),
            ["vnp_ResponseCode"] = "00",
            ["vnp_TransactionNo"] = "12345",
            ["vnp_SecureHash"] = "definitelywrong"
        });

        var sut = CreateSut(orderRepo, paymentRepo);
        var ok = await sut.ProcessVnPayReturn(query);

        ok.Should().BeFalse();
        orderRepo.Verify(o => o.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }
}
