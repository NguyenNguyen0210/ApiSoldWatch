using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ShopNN.DTOs;
using ShopNN.Entities;
using ShopNN.Mappings;
using ShopNN.Repositories.Interface;
using ShopNN.Services.Implement;
using ShopNN.Shared.Exceptions;

namespace ShopNN.Tests.Services;

public class CartServiceTests
{
    private readonly Mock<ICartRepository>    _cartMock    = new();
    private readonly Mock<IProductRepository> _productMock = new();
    private readonly IMapper _mapper;
    private readonly CartService _sut;

    public CartServiceTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();
        _sut = new CartService(_cartMock.Object, _mapper, _productMock.Object, new Mock<ILogger<CartService>>().Object);
    }

    private static Product MakeProduct(int? id = null, int stock = 10) => new()
    {
        Id          = id ?? 1,
        Name        = "Product",
        Description = "Desc",
        Price       = 100,
        Stock       = stock,
        CategoryId  = 1
    };

    private static Cart MakeCart(Guid? userId = null, List<CartItem>? items = null) => new()
    {
        Id        = Guid.NewGuid(),
        UserId    = userId ?? Guid.NewGuid(),
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
        Items     = items ?? new List<CartItem>()
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

    #region GetCart
    [Fact]
    public async Task GetCartByIdAsync_WhenFound_ShouldReturnDto()
    {
        var cart = MakeCart();
        _cartMock.Setup(c => c.GetByIdAsync(cart.Id)).ReturnsAsync(cart);

        var result = await _sut.GetCartByIdAsync(cart.Id);

        result.Id.Should().Be(cart.Id);
    }

    [Fact]
    public async Task GetCartByUserIdAsync_WhenExists_ShouldReturnDto()
    {
        var uid  = Guid.NewGuid();
        var cart = MakeCart(uid);
        _cartMock.Setup(c => c.GetCartByUserIdAsync(uid)).ReturnsAsync(cart);

        var result = await _sut.GetCartByUserIdAsync(uid);

        result.UserId.Should().Be(uid);
    }
    #endregion

    #region AddItemToCartAsync
    [Fact]
    public async Task AddItemToCartAsync_WhenValid_ShouldAddItem()
    {
        var uid = Guid.NewGuid();
        var product = MakeProduct(id: 1, stock: 10);
        var cart = MakeCart(uid);

        _productMock.Setup(p => p.GetByIdAsync(product.Id)).ReturnsAsync(product);
        _cartMock.Setup(c => c.GetCartForUpdateAsync(uid)).ReturnsAsync(cart);
        _cartMock.Setup(c => c.GetCartByUserIdAsync(uid)).ReturnsAsync(cart);

        var dto = new CartItemRequestDTO { ProductId = product.Id, Quantity = 2 };

        var result = await _sut.AddItemToCartAsync(uid, dto);

        cart.Items.Should().HaveCount(1);
        cart.Items.First().Quantity.Should().Be(2);
        _cartMock.Verify(c => c.SaveChangeAsync(), Times.Once);
    }

    [Fact]
    public async Task AddItemToCartAsync_WhenItemExists_ShouldAccumulate()
    {
        var uid = Guid.NewGuid();
        var product = MakeProduct(id: 1, stock: 10);
        var cart = MakeCart(uid);
        var item = MakeCartItem(cart, product, quantity: 2);
        cart.Items.Add(item);

        _productMock.Setup(p => p.GetByIdAsync(product.Id)).ReturnsAsync(product);
        _cartMock.Setup(c => c.GetCartForUpdateAsync(uid)).ReturnsAsync(cart);
        _cartMock.Setup(c => c.GetCartByUserIdAsync(uid)).ReturnsAsync(cart);

        var dto = new CartItemRequestDTO { ProductId = product.Id, Quantity = 3 };

        await _sut.AddItemToCartAsync(uid, dto);

        item.Quantity.Should().Be(5);
    }

    [Fact]
    public async Task AddItemToCartAsync_WhenExceedsStock_ShouldThrow()
    {
        var uid = Guid.NewGuid();
        var product = MakeProduct(id: 1, stock: 5);
        _productMock.Setup(p => p.GetByIdAsync(product.Id)).ReturnsAsync(product);
        _cartMock.Setup(c => c.GetCartForUpdateAsync(uid)).ReturnsAsync(MakeCart(uid));

        var act = async () => await _sut.AddItemToCartAsync(uid, new CartItemRequestDTO { ProductId = 1, Quantity = 10 });

        await act.Should().ThrowAsync<BadRequestException>();
    }
    #endregion

    #region UpdateItemQuantityAsync
    [Fact]
    public async Task UpdateItemQuantityAsync_WhenValid_ShouldUpdate()
    {
        var uid = Guid.NewGuid();
        var product = MakeProduct(stock: 10);
        var cart = MakeCart(uid);
        var item = MakeCartItem(cart, product, quantity: 1);
        cart.Items.Add(item);

        _productMock.Setup(p => p.GetByIdAsync(product.Id)).ReturnsAsync(product);
        _cartMock.Setup(c => c.GetCartForUpdateAsync(uid)).ReturnsAsync(cart);
        _cartMock.Setup(c => c.GetCartByUserIdAsync(uid)).ReturnsAsync(cart);

        await _sut.UpdateItemQuantityAsync(uid, item.Id, new CartItemUpdateDTO { Quantity = 5 });

        item.Quantity.Should().Be(5);
        _cartMock.Verify(c => c.SaveChangeAsync(), Times.Once);
    }
    #endregion

    #region RemoveItemFromCartAsync
    [Fact]
    public async Task RemoveItemFromCartAsync_WhenValid_ShouldRemove()
    {
        var uid = Guid.NewGuid();
        var cart = MakeCart(uid);
        var item = MakeCartItem(cart, MakeProduct());
        cart.Items.Add(item);

        _cartMock.Setup(c => c.GetCartForUpdateAsync(uid)).ReturnsAsync(cart);
        _cartMock.Setup(c => c.GetCartByUserIdAsync(uid)).ReturnsAsync(cart);

        await _sut.RemoveItemFromCartAsync(uid, item.Id);

        _cartMock.Verify(c => c.DeleteItemAsync(item.Id), Times.Once);
        _cartMock.Verify(c => c.SaveChangeAsync(), Times.Once);
    }
    #endregion

    #region ClearCartAsync
    [Fact]
    public async Task ClearCartAsync_WhenNotEmpty_ShouldClear()
    {
        var uid = Guid.NewGuid();
        var cart = MakeCart(uid);
        cart.Items.Add(MakeCartItem(cart, MakeProduct()));

        _cartMock.Setup(c => c.GetCartForUpdateAsync(uid)).ReturnsAsync(cart);

        await _sut.ClearCartAsync(uid);

        _cartMock.Verify(c => c.ClearCartAsync(cart.Id), Times.Once);
        _cartMock.Verify(c => c.SaveChangeAsync(), Times.Once);
    }

    [Fact]
    public async Task ClearCartAsync_WhenEmpty_ShouldThrow()
    {
        var uid = Guid.NewGuid();
        var cart = MakeCart(uid);

        _cartMock.Setup(c => c.GetCartForUpdateAsync(uid)).ReturnsAsync(cart);

        var act = async () => await _sut.ClearCartAsync(uid);

        await act.Should().ThrowAsync<BadRequestException>();
    }
    #endregion

    #region GetCart — Edge Cases
    [Fact]
    public async Task GetCartByIdAsync_WhenNotFound_ShouldThrowNotFoundException()
    {
        var id = Guid.NewGuid();
        _cartMock.Setup(c => c.GetByIdAsync(id)).ReturnsAsync((Cart?)null);

        var act = async () => await _sut.GetCartByIdAsync(id);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetCartByUserIdAsync_WhenNotExists_ShouldCreateNewCart()
    {
        var uid = Guid.NewGuid();
        _cartMock.Setup(c => c.GetCartByUserIdAsync(uid)).ReturnsAsync((Cart?)null);
        _cartMock.Setup(c => c.AddAsync(It.IsAny<Cart>())).ReturnsAsync((Cart c) => c);

        var result = await _sut.GetCartByUserIdAsync(uid);

        result.UserId.Should().Be(uid);
        _cartMock.Verify(c => c.AddAsync(It.IsAny<Cart>()), Times.Once);
    }
    #endregion

    #region AddItemToCartAsync — Edge Cases
    [Fact]
    public async Task AddItemToCartAsync_WhenProductNotFound_ShouldThrow()
    {
        var uid = Guid.NewGuid();
        _productMock.Setup(p => p.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Product?)null);
        _cartMock.Setup(c => c.GetCartForUpdateAsync(uid)).ReturnsAsync(MakeCart(uid));

        var act = async () => await _sut.AddItemToCartAsync(uid, new CartItemRequestDTO { ProductId = 999, Quantity = 1 });

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("Product not found");
    }

    [Fact]
    public async Task AddItemToCartAsync_WhenQuantityZero_ShouldThrow()
    {
        var uid = Guid.NewGuid();

        var act = async () => await _sut.AddItemToCartAsync(uid, new CartItemRequestDTO { ProductId = 1, Quantity = 0 });

        await act.Should().ThrowAsync<BadRequestException>();
    }
    #endregion

    #region UpdateItemQuantityAsync — Edge Cases
    [Fact]
    public async Task UpdateItemQuantityAsync_WhenCartNotFound_ShouldThrow()
    {
        var uid = Guid.NewGuid();
        _cartMock.Setup(c => c.GetCartForUpdateAsync(uid)).ReturnsAsync((Cart?)null);

        var act = async () => await _sut.UpdateItemQuantityAsync(uid, Guid.NewGuid(), new CartItemUpdateDTO { Quantity = 1 });

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("Cart Not Found");
    }

    [Fact]
    public async Task UpdateItemQuantityAsync_WhenItemNotFound_ShouldThrow()
    {
        var uid = Guid.NewGuid();
        var cart = MakeCart(uid);
        _cartMock.Setup(c => c.GetCartForUpdateAsync(uid)).ReturnsAsync(cart);

        var act = async () => await _sut.UpdateItemQuantityAsync(uid, Guid.NewGuid(), new CartItemUpdateDTO { Quantity = 1 });

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("Cart item not found");
    }

    [Fact]
    public async Task UpdateItemQuantityAsync_WhenExceedsStock_ShouldThrow()
    {
        var uid = Guid.NewGuid();
        var product = MakeProduct(stock: 5);
        var cart = MakeCart(uid);
        var item = MakeCartItem(cart, product, quantity: 1);
        cart.Items.Add(item);

        _productMock.Setup(p => p.GetByIdAsync(product.Id)).ReturnsAsync(product);
        _cartMock.Setup(c => c.GetCartForUpdateAsync(uid)).ReturnsAsync(cart);

        var act = async () => await _sut.UpdateItemQuantityAsync(uid, item.Id, new CartItemUpdateDTO { Quantity = 100 });

        await act.Should().ThrowAsync<BadRequestException>().WithMessage("Not enough stock");
    }
    #endregion
}
