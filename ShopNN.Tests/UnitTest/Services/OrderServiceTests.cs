using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Moq;
using ShopNN.DTOs.Account;
using ShopNN.DTOs.Product;
using ShopNN.DTOs.Category;
using ShopNN.DTOs.Cart;
using ShopNN.DTOs.Order;
using ShopNN.Entities;
using ShopNN.Mappings;
using ShopNN.Repositories.Interface;
using ShopNN.Services.Implement;
using ShopNN.Shared.Enums;
using ShopNN.Shared.Exceptions;
using ShopNN.Shared.Wrappers;

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
        int[] a = [1,2, 3];
        
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();
        _sut = new OrderService(_orderRepo.Object, _cartRepo.Object, _productRepo.Object, _mapper, new Mock<ILogger<OrderService>>().Object);
    }

    private static Product MakeProduct(int? id = null, int stock = 10, decimal price = 99) => new()
    {
        Id          = id ?? 1,
        Name        = "Test Product",
        Description = "Desc",
        Price       = price,
        Stock       = stock,
        CategoryId  = 1
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

    private static OrderCreateRequestDTO MakeCreateRequest(PaymentMethod method = PaymentMethod.COD) => new()
    {
        PaymentMethod = method,
        ReceiverName = "Nguyen Nguyen",
        PhoneNumber = "0987654321",
        ShippingAddress = "123 Main St, Ward 5, District 10, HCMC"
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
    public async Task CreateOrderAsync_WhenCartEmpty_ShouldThrowBadRequestException()
    {
        var cart = MakeCart(); 
        _cartRepo.Setup(c => c.GetCartByUserIdAsync(It.IsAny<Guid>()))
                 .ReturnsAsync(cart);

        var act = async () => await _sut.CreateOrderAsync(Guid.NewGuid(), MakeCreateRequest(PaymentMethod.COD));

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
            ProductId = 1,
            Cart      = cart
        });

        _cartRepo.Setup(c => c.GetCartByUserIdAsync(uid)).ReturnsAsync(cart);
        var tx = SetupTransaction();

        var act = async () => await _sut.CreateOrderAsync(uid, MakeCreateRequest(PaymentMethod.COD));

        await act.Should().ThrowAsync<NotFoundException>()
                 .WithMessage("Product not found.");
        tx.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateOrderAsync_WhenValid_ShouldDeductStockFromProducts()
    {
        var uid = Guid.NewGuid();
        var product = MakeProduct(id: 1, stock: 10, price: 100);
        var cart = MakeCart(uid);
        cart.Items.Add(MakeCartItem(cart, product, quantity: 3));

        _cartRepo.Setup(c => c.GetCartByUserIdAsync(uid)).ReturnsAsync(cart);
        _orderRepo.Setup(o => o.AddAsync(It.IsAny<Order>())).ReturnsAsync((Order o) => o);
        _orderRepo.Setup(o => o.SaveChangesAsync()).Returns(Task.CompletedTask);
        SetupTransaction();

        var request = MakeCreateRequest(PaymentMethod.COD);
        var result = await _sut.CreateOrderAsync(uid, request);

        product.Stock.Should().Be(7); 
        _orderRepo.Verify(o => o.AddAsync(It.Is<Order>(o => 
            o.ReceiverName == request.ReceiverName && 
            o.PhoneNumber == request.PhoneNumber && 
            o.ShippingAddress == request.ShippingAddress)), Times.Once);
        result.ReceiverName.Should().Be(request.ReceiverName);
        result.PhoneNumber.Should().Be(request.PhoneNumber);
        result.ShippingAddress.Should().Be(request.ShippingAddress);
    }
    #endregion

    #region GetMyOrdersAsync
    [Fact]
    public async Task GetMyOrdersAsync_WhenCalled_ShouldReturnMappedOrders()
    {
        var uid   = Guid.NewGuid();
        var order = MakeOrder(uid);
        _orderRepo.Setup(o => o.GetByUserIdAsync(uid))
                  .ReturnsAsync(new List<Order> { order });

        var result = await _sut.GetMyOrdersAsync(uid);

        result.Should().ContainSingle(o => o.Id == order.Id);
    }
    #endregion

    #region GetAllOrdersAsync
    [Fact]
    public async Task GetAllOrdersAsync_WhenCalled_ShouldReturnAllOrders()
    {
        var orders = new List<Order> { MakeOrder(), MakeOrder() };
        _orderRepo.Setup(o => o.GetAllAsync()).ReturnsAsync(orders);

        var result = await _sut.GetAllOrdersAsync();

        result.Should().HaveCount(2);
        _orderRepo.Verify(o => o.GetAllAsync(), Times.Once);
    }
    #endregion

    #region UpdateStatusAsync
    [Fact]
    public async Task UpdateStatusAsync_WhenExists_ShouldUpdateStatus()
    {
        var orderId = Guid.NewGuid();
        var order = MakeOrder();
        order.Id = orderId;
        order.Status = OrderStatus.Pending;

        _orderRepo.Setup(o => o.GetByIdAsync(orderId)).ReturnsAsync(order);
        _orderRepo.Setup(o => o.SaveChangesAsync()).Returns(Task.CompletedTask);

        var result = await _sut.UpdateStatusAsync(orderId, OrderStatus.Processing);

        order.Status.Should().Be(OrderStatus.Processing);
        result.Status.Should().Be(OrderStatus.Processing.ToString());
        _orderRepo.Verify(o => o.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateStatusAsync_WhenNotFound_ShouldThrowNotFoundException()
    {
        _orderRepo.Setup(o => o.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Order?)null);

        var act = async () => await _sut.UpdateStatusAsync(Guid.NewGuid(), OrderStatus.Delivered);

        await act.Should().ThrowAsync<NotFoundException>();
    }
    #endregion

    #region CreateOrderAsync — Edge Cases
    [Fact]
    public async Task CreateOrderAsync_WhenCartNull_ShouldThrowBadRequestException()
    {
        _cartRepo.Setup(c => c.GetCartByUserIdAsync(It.IsAny<Guid>()))
                 .ReturnsAsync((Cart?)null);

        var act = async () => await _sut.CreateOrderAsync(Guid.NewGuid(), MakeCreateRequest(PaymentMethod.COD));

        await act.Should().ThrowAsync<BadRequestException>()
                 .WithMessage("Your cart is empty.");
    }

    [Fact]
    public async Task CreateOrderAsync_WhenOutOfStock_ShouldRollbackAndThrow()
    {
        var uid = Guid.NewGuid();
        var product = MakeProduct(id: 1, stock: 2, price: 100);
        var cart = MakeCart(uid);
        cart.Items.Add(MakeCartItem(cart, product, quantity: 5));

        _cartRepo.Setup(c => c.GetCartByUserIdAsync(uid)).ReturnsAsync(cart);
        var tx = SetupTransaction();

        var act = async () => await _sut.CreateOrderAsync(uid, MakeCreateRequest(PaymentMethod.COD));

        await act.Should().ThrowAsync<BadRequestException>()
                 .WithMessage("*out of stock*");
        tx.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateOrderAsync_WhenValid_ShouldCalculateTotalAmountCorrectly()
    {
        var uid = Guid.NewGuid();
        var product1 = MakeProduct(id: 1, stock: 10, price: 100);
        var product2 = MakeProduct(id: 2, stock: 10, price: 200);
        var cart = MakeCart(uid);
        cart.Items.Add(MakeCartItem(cart, product1, quantity: 2));
        cart.Items.Add(MakeCartItem(cart, product2, quantity: 3));

        _cartRepo.Setup(c => c.GetCartByUserIdAsync(uid)).ReturnsAsync(cart);
        _orderRepo.Setup(o => o.AddAsync(It.IsAny<Order>())).ReturnsAsync((Order o) => o);
        _orderRepo.Setup(o => o.SaveChangesAsync()).Returns(Task.CompletedTask);
        SetupTransaction();

        var result = await _sut.CreateOrderAsync(uid, MakeCreateRequest(PaymentMethod.VnPay));

        result.TotalAmount.Should().Be(2 * 100 + 3 * 200);
        result.Items.Should().HaveCount(2);
    }
    #endregion

    #region GetAllOrdersPagedAsync
    private static PagedResult<Order> MakePagedOrders(
        List<Order>? items = null, int page = 1, int pageSize = 10, int totalCount = 0)
    {
        var list = items ?? new List<Order>();
        return new PagedResult<Order>
        {
            Items = list,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount == 0 ? list.Count : totalCount
        };
    }

    [Fact]
    public async Task GetAllOrdersPagedAsync_WhenDefaultQuery_ShouldReturnPagedResult()
    {
        var orders = new List<Order> { MakeOrder(), MakeOrder(), MakeOrder() };
        var query = new OrderQueryDTO();

        _orderRepo.Setup(o => o.GetPagedAsync(query))
                  .ReturnsAsync(MakePagedOrders(orders, page: 1, pageSize: 10, totalCount: 3));

        var result = await _sut.GetAllOrdersPagedAsync(query);

        result.Items.Should().HaveCount(3);
        result.Page.Should().Be(1);
        result.TotalCount.Should().Be(3);
        result.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task GetAllOrdersPagedAsync_WhenFilterByStatus_ShouldPassToRepo()
    {
        var query = new OrderQueryDTO { Status = OrderStatus.Pending };

        _orderRepo.Setup(o => o.GetPagedAsync(It.Is<OrderQueryDTO>(q => q.Status == OrderStatus.Pending)))
                  .ReturnsAsync(MakePagedOrders(new List<Order> { MakeOrder() }, totalCount: 1));

        var result = await _sut.GetAllOrdersPagedAsync(query);

        result.Items.Should().HaveCount(1);
        _orderRepo.Verify(o => o.GetPagedAsync(It.Is<OrderQueryDTO>(q => q.Status == OrderStatus.Pending)), Times.Once);
    }

    [Fact]
    public async Task GetAllOrdersPagedAsync_WhenFilterByDateRange_ShouldPassToRepo()
    {
        var from = DateTime.UtcNow.AddDays(-7);
        var to = DateTime.UtcNow;
        var query = new OrderQueryDTO { FromDate = from, ToDate = to };

        _orderRepo.Setup(o => o.GetPagedAsync(It.Is<OrderQueryDTO>(q => q.FromDate == from && q.ToDate == to)))
                  .ReturnsAsync(MakePagedOrders(new List<Order> { MakeOrder() }, totalCount: 1));

        var result = await _sut.GetAllOrdersPagedAsync(query);

        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAllOrdersPagedAsync_WhenNoResults_ShouldReturnEmptyPage()
    {
        var query = new OrderQueryDTO { Status = OrderStatus.Cancelled };

        _orderRepo.Setup(o => o.GetPagedAsync(query))
                  .ReturnsAsync(MakePagedOrders());

        var result = await _sut.GetAllOrdersPagedAsync(query);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task GetAllOrdersPagedAsync_WhenMultiplePages_ShouldReturnCorrectPagination()
    {
        var orders = new List<Order> { MakeOrder(), MakeOrder() };
        var query = new OrderQueryDTO { Page = 2, PageSize = 2 };

        _orderRepo.Setup(o => o.GetPagedAsync(query))
                  .ReturnsAsync(MakePagedOrders(orders, page: 2, pageSize: 2, totalCount: 7));

        var result = await _sut.GetAllOrdersPagedAsync(query);

        result.Page.Should().Be(2);
        result.PageSize.Should().Be(2);
        result.TotalCount.Should().Be(7);
        result.TotalPages.Should().Be(4);
        result.HasPreviousPage.Should().BeTrue();
        result.HasNextPage.Should().BeTrue();
    }
    #endregion
}

