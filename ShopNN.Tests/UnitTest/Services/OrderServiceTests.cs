using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using ShopNN.Entities;
using ShopNN.Mappings;
using ShopNN.Repositories.Interface;
using ShopNN.Services.Implement;
using ShopNN.Shared.Enums;
using ShopNN.Shared.Exeptions;

namespace ShopNN.Tests.Services;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository>   _orderRepo   = new();
    private readonly Mock<ICartRepository>    _cartRepo    = new();
    private readonly Mock<IProductRepository> _productRepo = new();
    private readonly IMapper _mapper;
    private readonly OrderService _sut;

    public OrderServiceTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();
        _sut = new OrderService(_orderRepo.Object, _cartRepo.Object, _productRepo.Object, _mapper);
    }

    private static Product MakeProduct(Guid? id = null, int stock = 10, decimal price = 99) => new()
    {
        Id          = id ?? Guid.NewGuid(),
        Name        = "Test Product",
        Description = "Desc",
        Price       = price,
        Stock       = stock,
        CategoryId  = Guid.NewGuid()
    };

    private static Cart MakeCart(Guid? userId = null) => new()
    {
        Id        = Guid.NewGuid(),
        UserId    = userId ?? Guid.NewGuid(),
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
        Items     = new List<CartItem>()
    };

    private static CartItem MakeCartItem(Cart cart, Product product, int quantity = 2) => new()
    {
        Id        = Guid.NewGuid(),
        CartId    = cart.Id,
        ProductId = product.Id,
        Product   = product,
        Cart      = cart,
        Quantity  = quantity
    };

    private static Order MakeOrder(Guid? userId = null) => new()
    {
        Id            = Guid.NewGuid(),
        UserId        = userId ?? Guid.NewGuid(),
        Status        = OrderStatus.Pending,
        PaymentStatus = PaymentStatus.Unpaid,
        PaymentMethod = PaymentMethod.COD,
        TotalAmount   = 0,
        CreatedAt     = DateTime.UtcNow,
        Items         = new List<OrderItem>()
    };

    private Mock<IDbContextTransaction> SetupTransaction()
    {
        var tx = new Mock<IDbContextTransaction>();
        tx.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>()))
          .Returns(Task.CompletedTask);
        tx.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>()))
          .Returns(Task.CompletedTask);
        tx.Setup(t => t.DisposeAsync())
          .Returns(ValueTask.CompletedTask);
        _orderRepo.Setup(o => o.BeginTransactionAsync())
                  .ReturnsAsync(tx.Object);
        return tx;
    }
    #region CreateOrderAsync
    [Fact]
    public async Task CreateOrderAsync_WhenCartNull_ShouldThrowBadRequestException()
    {
        _cartRepo.Setup(c => c.GetCartByUserIdAsync(It.IsAny<Guid>()))
                 .ReturnsAsync((Cart?)null);

        var act = async () => await _sut.CreateOrderAsync(Guid.NewGuid(), PaymentMethod.COD);

        await act.Should().ThrowAsync<BadRequestException>()
                 .WithMessage("Your cart is empty.");
    }

    [Fact]
    public async Task CreateOrderAsync_WhenCartEmpty_ShouldThrowBadRequestException()
    {
        var cart = MakeCart(); 
        _cartRepo.Setup(c => c.GetCartByUserIdAsync(It.IsAny<Guid>()))
                 .ReturnsAsync(cart);

        var act = async () => await _sut.CreateOrderAsync(Guid.NewGuid(), PaymentMethod.COD);

        await act.Should().ThrowAsync<BadRequestException>()
                 .WithMessage("Your cart is empty.");
    }

    [Fact]
    public async Task CreateOrderAsync_WhenProductNavigationNull_ShouldRollbackAndThrow()
    {
        var uid  = Guid.NewGuid();
        var cart = MakeCart(uid);
        cart.Items.Add(new CartItem
        {
            Id        = Guid.NewGuid(),
            CartId    = cart.Id,
            Quantity  = 1,
            Product   = null,
            ProductId = Guid.NewGuid(),
            Cart      = cart
        });

        _cartRepo.Setup(c => c.GetCartByUserIdAsync(uid)).ReturnsAsync(cart);
        var tx = SetupTransaction();

        var act = async () => await _sut.CreateOrderAsync(uid, PaymentMethod.COD);

        await act.Should().ThrowAsync<NotFoundException>()
                 .WithMessage("Product not found.");
        tx.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        tx.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateOrderAsync_WhenStockInsufficient_ShouldRollbackAndThrow()
    {
        var uid     = Guid.NewGuid();
        var product = MakeProduct(stock: 2);
        var cart    = MakeCart(uid);
        cart.Items.Add(MakeCartItem(cart, product, quantity: 5));

        _cartRepo.Setup(c => c.GetCartByUserIdAsync(uid)).ReturnsAsync(cart);
        var tx = SetupTransaction();

        var act = async () => await _sut.CreateOrderAsync(uid, PaymentMethod.COD);

        await act.Should().ThrowAsync<BadRequestException>()
                 .WithMessage($"*{product.Name}*");
        tx.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        tx.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateOrderAsync_WhenValid_ShouldCommitAndReturnOrder()
    {
        var uid     = Guid.NewGuid();
        var product = MakeProduct(stock: 10, price: 99);
        var cart    = MakeCart(uid);
        cart.Items.Add(MakeCartItem(cart, product, quantity: 2));

        _cartRepo.Setup(c => c.GetCartByUserIdAsync(uid)).ReturnsAsync(cart);

        Order? persisted = null;
        _orderRepo.Setup(o => o.AddAsync(It.IsAny<Order>()))
                  .Callback<Order>(o => persisted = o).ReturnsAsync((Order o) => o);
        
        var tx = SetupTransaction();

        var result = await _sut.CreateOrderAsync(uid, PaymentMethod.VnPay);

        result.TotalAmount.Should().Be(198);

        persisted.Should().NotBeNull();
        persisted!.UserId.Should().Be(uid);
        persisted.Items.Should().HaveCount(1);

        cart.Items.Should().BeEmpty();

        _orderRepo.Verify(o => o.AddAsync(It.IsAny<Order>()), Times.Once);
        
        tx.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        tx.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateOrderAsync_WhenValid_ShouldDeductStock()
    {
        var uid     = Guid.NewGuid();
        var product = MakeProduct(stock: 10);
        var cart    = MakeCart(uid);
        cart.Items.Add(MakeCartItem(cart, product, quantity: 3));

        _cartRepo.Setup(c => c.GetCartByUserIdAsync(uid)).ReturnsAsync(cart);
        _orderRepo.Setup(o => o.AddAsync(It.IsAny<Order>())).ReturnsAsync((Order o) => o);
        _orderRepo.Setup(o => o.SaveChangesAsync()).Returns(Task.CompletedTask);
        SetupTransaction();

        await _sut.CreateOrderAsync(uid, PaymentMethod.COD);

        product.Stock.Should().Be(7);
    }

    [Fact]
    public async Task CreateOrderAsync_WhenValid_ShouldSnapshotUnitPrice()
    {
        var uid     = Guid.NewGuid();
        var product = MakeProduct(stock: 10, price: 150);
        var cart    = MakeCart(uid);
        cart.Items.Add(MakeCartItem(cart, product, quantity: 2));

        _cartRepo.Setup(c => c.GetCartByUserIdAsync(uid)).ReturnsAsync(cart);
        Order? persisted = null;
        _orderRepo.Setup(o => o.AddAsync(It.IsAny<Order>()))
                  .Callback<Order>(o => persisted = o).ReturnsAsync((Order o) => o);
        _orderRepo.Setup(o => o.SaveChangesAsync()).Returns(Task.CompletedTask);
        SetupTransaction();

        await _sut.CreateOrderAsync(uid, PaymentMethod.COD);

        persisted!.Items.First().UnitPrice.Should().Be(150);
    }
    #endregion
    #region GetMyOrdersAsync
    [Fact]
    public async Task GetMyOrdersAsync_WhenCalled_ShouldReturnMappedOrders()
    {
        var uid   = Guid.NewGuid();
        var order = MakeOrder(uid);
        order.TotalAmount = 200;

        _orderRepo.Setup(o => o.GetByUserIdAsync(uid))
                  .ReturnsAsync(new List<Order> { order });

        var result = await _sut.GetMyOrdersAsync(uid);

        result.Should().ContainSingle(o => o.Id == order.Id && o.TotalAmount == 200);
        _orderRepo.Verify(o => o.GetByUserIdAsync(uid), Times.Once);
    }

    [Fact]
    public async Task GetMyOrdersAsync_WhenNoOrders_ShouldReturnEmptyList()
    {
        _orderRepo.Setup(o => o.GetByUserIdAsync(It.IsAny<Guid>()))
                  .ReturnsAsync(new List<Order>());

        var result = await _sut.GetMyOrdersAsync(Guid.NewGuid());

        result.Should().BeEmpty();
    }
    #endregion
    #region GetAllOrdersAsync
    [Fact]
    public async Task GetAllOrdersAsync_WhenCalled_ShouldReturnAllMappedOrders()
    {
        var orders = new List<Order> { MakeOrder(), MakeOrder() };
        _orderRepo.Setup(o => o.GetAllAsync()).ReturnsAsync(orders);

        var result = await _sut.GetAllOrdersAsync();

        result.Should().HaveCount(2);
        _orderRepo.Verify(o => o.GetAllAsync(), Times.Once);
    }
    #endregion
    #region GetAllOrdersAsync
    [Fact]
    public async Task GetAllOrdersAsync_WhenEmpty_ShouldReturnEmptyList()
    {
        _orderRepo.Setup(o => o.GetAllAsync()).ReturnsAsync(new List<Order>());

        var result = await _sut.GetAllOrdersAsync();

        result.Should().BeEmpty();
    }
    #endregion
    #region UpdateStatusAsync
    [Fact]
    public async Task UpdateStatusAsync_WhenOrderNotFound_ShouldThrowNotFoundException()
    {
        _orderRepo.Setup(o => o.GetByIdAsync(It.IsAny<Guid>()))
                  .ReturnsAsync((Order?)null);

        var act = async () => await _sut.UpdateStatusAsync(Guid.NewGuid(), OrderStatus.Delivered);

        await act.Should().ThrowAsync<NotFoundException>()
                 .WithMessage("Order not found.");
        _orderRepo.Verify(o => o.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateStatusAsync_WhenFound_ShouldUpdateStatusAndSave()
    {
        var order = MakeOrder();
        order.Status = OrderStatus.Pending;

        _orderRepo.Setup(o => o.GetByIdAsync(order.Id)).ReturnsAsync(order);
        _orderRepo.Setup(o => o.SaveChangesAsync()).Returns(Task.CompletedTask);

        var result = await _sut.UpdateStatusAsync(order.Id, OrderStatus.Processing);

        result.Id.Should().Be(order.Id);
        order.Status.Should().Be(OrderStatus.Processing);
        _orderRepo.Verify(o => o.SaveChangesAsync(), Times.Once);
    }
    #endregion
}


