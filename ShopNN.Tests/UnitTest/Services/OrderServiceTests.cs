using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using ShopNN.DTOs;
using ShopNN.Entities;
using ShopNN.Mappings;
using ShopNN.Repositories.Interface;
using ShopNN.Services.Implement;
using ShopNN.Shared.Enums;
using ShopNN.Shared.Exeptions;

namespace ShopNN.Tests.Services;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepo;
    private readonly Mock<ICartRepository> _cartRepo;
    private readonly Mock<IProductRepository> _productRepo;
    private readonly IMapper _mapper;
    private readonly OrderService _sut;

    public OrderServiceTests()
    {
        _orderRepo = new Mock<IOrderRepository>();
        _cartRepo = new Mock<ICartRepository>();
        _productRepo = new Mock<IProductRepository>();
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();
        _sut = new OrderService(_orderRepo.Object, _cartRepo.Object, _productRepo.Object, _mapper);
    }

    private static Cart CartWithReadyItem(Guid userId, int qty, int stock, string productName)
    {
        var pid = Guid.NewGuid();
        var cartId = Guid.NewGuid();
        var prod = new Product { Id = pid, Name = productName, Description = "D", Price = 99, Stock = stock };
        var cart = new Cart { Id = cartId, UserId = userId };
        cart.Items.Add(new CartItem
        {
            Id = Guid.NewGuid(),
            CartId = cart.Id,
            ProductId = pid,
            Quantity = qty,
            Product = prod,
            Cart = cart
        });
        return cart;
    }

    private Mock<IDbContextTransaction> BindTransactionSetup()
    {
        var tx = new Mock<IDbContextTransaction>();
        tx.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        tx.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        tx.Setup(t => t.Dispose());
        _orderRepo.Setup(o => o.BeginTransactionAsync()).ReturnsAsync(tx.Object);
        return tx;
    }

    [Fact]
    public async Task CreateOrderAsync_WhenCartEmpty_ShouldThrow()
    {
        _cartRepo.Setup(c => c.GetCartByUserIdAsync(It.IsAny<Guid>())).ReturnsAsync(new Cart { Items = new() });

        var act = async () => await _sut.CreateOrderAsync(Guid.NewGuid(), PaymentMethod.COD);

        await act.Should().ThrowAsync<BadRequestException>().WithMessage("Your cart is empty.");
    }

    [Fact]
    public async Task CreateOrderAsync_WhenCartNull_ShouldThrow()
    {
        _cartRepo.Setup(c => c.GetCartByUserIdAsync(It.IsAny<Guid>())).ReturnsAsync((Cart?)null);

        var act = async () => await _sut.CreateOrderAsync(Guid.NewGuid(), PaymentMethod.COD);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task CreateOrderAsync_WhenProductNavigationMissing_ShouldRollbackAndThrow()
    {
        var uid = Guid.NewGuid();
        var cartId = Guid.NewGuid();
        var cart = new Cart { Id = cartId, UserId = uid };
        cart.Items.Add(new CartItem
        {
            Id = Guid.NewGuid(),
            CartId = cartId,
            Quantity = 1,
            Product = null,
            ProductId = Guid.NewGuid(),
            Cart = cart
        });
        _cartRepo.Setup(c => c.GetCartByUserIdAsync(uid)).ReturnsAsync(cart);
        var tx = BindTransactionSetup();

        var act = async () => await _sut.CreateOrderAsync(uid, PaymentMethod.COD);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("Product not found.");
        tx.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task CreateOrderAsync_WhenStockTooLow_ShouldRollback()
    {
        var uid = Guid.NewGuid();
        var cart = CartWithReadyItem(uid, 5, 2, "X");
        _cartRepo.Setup(c => c.GetCartByUserIdAsync(uid)).ReturnsAsync(cart);
        var tx = BindTransactionSetup();

        var act = async () => await _sut.CreateOrderAsync(uid, PaymentMethod.COD);

        await act.Should().ThrowAsync<BadRequestException>();
        tx.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task CreateOrderAsync_WhenValid_ShouldCommitAndPersist()
    {
        var uid = Guid.NewGuid();
        var cart = CartWithReadyItem(uid, 2, 10, "Widget");
        _cartRepo.Setup(c => c.GetCartByUserIdAsync(uid)).ReturnsAsync(cart);
        BindTransactionSetup();
        Order? persisted = null;
        _orderRepo.Setup(o => o.AddAsync(It.IsAny<Order>())).Callback<Order>(o => persisted = o)
            .ReturnsAsync((Order o) => o);
        _orderRepo.Setup(o => o.SaveChangesAsync()).Returns(Task.CompletedTask);

        var dto = await _sut.CreateOrderAsync(uid, PaymentMethod.VnPay);

        dto.TotalAmount.Should().Be(198); /* 99 * 2 */
        persisted.Should().NotBeNull();
        cart.Items.Should().BeEmpty();
        _orderRepo.Verify(o => o.AddAsync(It.IsAny<Order>()), Times.Once);
        _orderRepo.Verify(o => o.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task GetMyOrdersAsync_ShouldMapFromRepo()
    {
        var uid = Guid.NewGuid();
        var oid = Guid.NewGuid();
        var orders = new List<Order>
        {
            new Order
            {
                Id = oid,
                UserId = uid,
                CreatedAt = DateTime.UtcNow,
                TotalAmount = 10,
                Status = OrderStatus.Pending,
                PaymentMethod = PaymentMethod.COD,
                PaymentStatus = PaymentStatus.Unpaid,
                Items = new List<OrderItem>()
            }
        };
        _orderRepo.Setup(o => o.GetByUserIdAsync(uid)).ReturnsAsync(orders);

        var dtos = await _sut.GetMyOrdersAsync(uid);

        dtos.Should().ContainSingle(o => o.Id == oid && o.TotalAmount == 10);
    }

    [Fact]
    public async Task GetAllOrdersAsync_ShouldMapAll()
    {
        _orderRepo.Setup(o => o.GetAllAsync()).ReturnsAsync(new List<Order>());

        await _sut.GetAllOrdersAsync();

        _orderRepo.Verify(o => o.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateStatusAsync_WhenMissing_ShouldThrow()
    {
        _orderRepo.Setup(o => o.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((global::Order?)null);

        var act = async () => await _sut.UpdateStatusAsync(Guid.NewGuid(), OrderStatus.Delivered);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("Order not found.");
    }

    [Fact]
    public async Task UpdateStatusAsync_WhenFound_ShouldUpdate()
    {
        var id = Guid.NewGuid();
        var order = new global::Order { Id = id, Status = OrderStatus.Pending };
        _orderRepo.Setup(o => o.GetByIdAsync(id)).ReturnsAsync(order);
        _orderRepo.Setup(o => o.SaveChangesAsync()).Returns(Task.CompletedTask);

        var dto = await _sut.UpdateStatusAsync(id, OrderStatus.Processing);

        dto.Id.Should().Be(id);
        order.Status.Should().Be(OrderStatus.Processing);
        _orderRepo.Verify(o => o.SaveChangesAsync(), Times.Once);
    }
}
