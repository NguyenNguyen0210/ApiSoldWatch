using AutoMapper;
using FluentAssertions;
using Moq;
using ShopNN.DTOs;
using ShopNN.Entities;
using ShopNN.Mappings;
using ShopNN.Repositories.Interface;
using ShopNN.Services.Implement;
using ShopNN.Shared.Exeptions;

namespace ShopNN.Tests.Services;

public class CartServiceTests
{
    private readonly Mock<ICartRepository> _cartMock;
    private readonly Mock<IProductRepository> _productMock;
    private readonly IMapper _mapper;
    private readonly CartService _sut;

    public CartServiceTests()
    {
        _cartMock = new Mock<ICartRepository>();
        _productMock = new Mock<IProductRepository>();
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();
        _sut = new CartService(_cartMock.Object, _mapper, _productMock.Object);
    }

    private static Product StockProduct(Guid id, int stock) => new()
    {
        Id = id,
        Name = "Product",
        Description = "D",
        Price = 100,
        Stock = stock
    };

    [Fact]
    public async Task GetCartByIdAsync_WhenNotFound_ShouldThrow()
    {
        _cartMock.Setup(c => c.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Cart?)null);

        var act = async () => await _sut.GetCartByIdAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetCartByIdAsync_WhenFound_ShouldReturnDto()
    {
        var cart = new Cart { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Items = new() };
        _cartMock.Setup(c => c.GetByIdAsync(cart.Id)).ReturnsAsync(cart);

        var dto = await _sut.GetCartByIdAsync(cart.Id);

        dto.Id.Should().Be(cart.Id);
    }

    [Fact]
    public async Task GetCartByUserIdAsync_WhenExists_ShouldReturnDtoWithoutCreating()
    {
        var uid = Guid.NewGuid();
        var cart = new Cart { Id = Guid.NewGuid(), UserId = uid, Items = new() };
        _cartMock.Setup(c => c.GetCartByUserIdAsync(uid)).ReturnsAsync(cart);

        var dto = await _sut.GetCartByUserIdAsync(uid);

        dto.Id.Should().Be(cart.Id);
        _cartMock.Verify(c => c.AddAsync(It.IsAny<Cart>()), Times.Never);
    }

    [Fact]
    public async Task GetCartByUserIdAsync_WhenNull_ShouldPersistNewCart()
    {
        var uid = Guid.NewGuid();
        Cart? persisted = null;
        _cartMock.Setup(c => c.GetCartByUserIdAsync(uid)).ReturnsAsync((Cart?)null);
        _cartMock.Setup(c => c.AddAsync(It.IsAny<Cart>()))
            .Callback<Cart>(c => persisted = c)
            .ReturnsAsync((Cart c) => c);

        await _sut.GetCartByUserIdAsync(uid);

        persisted.Should().NotBeNull();
        persisted!.UserId.Should().Be(uid);
        _cartMock.Verify(c => c.AddAsync(It.Is<Cart>(x => x.UserId == uid)), Times.Once);
    }

    [Fact]
    public async Task AddItemToCartAsync_WhenQuantityNotPositive_ShouldThrowBadRequest()
    {
        var dto = new CartItemRequestDTO { ProductId = Guid.NewGuid(), Quantity = 0 };

        var act = async () => await _sut.AddItemToCartAsync(Guid.NewGuid(), dto);

        await act.Should().ThrowAsync<BadRequestException>().WithMessage("Quantity must be greater than 0");
    }

    [Fact]
    public async Task AddItemToCartAsync_WhenProductMissing_ShouldThrowNotFound()
    {
        var dto = new CartItemRequestDTO { ProductId = Guid.NewGuid(), Quantity = 2 };
        _productMock.Setup(p => p.GetByIdAsync(dto.ProductId)).ReturnsAsync((Product?)null);

        var act = async () => await _sut.AddItemToCartAsync(Guid.NewGuid(), dto);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("Product not found");
    }

    [Fact]
    public async Task AddItemToCartAsync_WhenStockInsufficient_ShouldThrow()
    {
        var pid = Guid.NewGuid();
        var dto = new CartItemRequestDTO { ProductId = pid, Quantity = 20 };
        _productMock.Setup(p => p.GetByIdAsync(pid)).ReturnsAsync(StockProduct(pid, 5));

        var act = async () => await _sut.AddItemToCartAsync(Guid.NewGuid(), dto);

        await act.Should().ThrowAsync<BadRequestException>().WithMessage("Not enough stock");
    }

    [Fact]
    public async Task AddItemToCartAsync_WhenNoCart_ShouldCreateCartAddItemAndReturnDto()
    {
        var uid = Guid.NewGuid();
        var pid = Guid.NewGuid();
        var product = StockProduct(pid, 10);
        var dto = new CartItemRequestDTO { ProductId = pid, Quantity = 2 };
        _productMock.Setup(p => p.GetByIdAsync(pid)).ReturnsAsync(product);

        Cart? cartRef = null;
        _cartMock.SetupSequence(c => c.GetCartByUserIdAsync(uid))
            .ReturnsAsync((Cart?)null)
            .ReturnsAsync(() => cartRef!);

        _cartMock.Setup(c => c.AddAsync(It.IsAny<Cart>()))
            .Callback<Cart>(c => cartRef = c)
            .ReturnsAsync((Cart c) => c);
        _cartMock.Setup(c => c.SaveChangeAsync()).Returns(Task.CompletedTask);

        var result = await _sut.AddItemToCartAsync(uid, dto);

        result.Items.Should().HaveCount(1);
        result.Items[0].Quantity.Should().Be(dto.Quantity);
        _cartMock.Verify(c => c.AddAsync(It.IsAny<Cart>()), Times.Once);
        _cartMock.Verify(c => c.SaveChangeAsync(), Times.Once);
    }

    [Fact]
    public async Task AddItemToCartAsync_WhenMerging_ShouldRejectWhenCombinedExceedsStock()
    {
        var uid = Guid.NewGuid();
        var pid = Guid.NewGuid();
        var product = StockProduct(pid, 10);
        var cartId = Guid.NewGuid();
        var existingCart = new Cart { Id = cartId, UserId = uid, Items = new List<CartItem>() };
        existingCart.Items.Add(new CartItem
        {
            Id = Guid.NewGuid(),
            CartId = cartId,
            ProductId = pid,
            Quantity = 6,
            Product = product,
            Cart = existingCart
        });

        _productMock.Setup(p => p.GetByIdAsync(pid)).ReturnsAsync(product);
        _cartMock.Setup(c => c.GetCartByUserIdAsync(uid)).ReturnsAsync(existingCart);

        var act = async () => await _sut.AddItemToCartAsync(uid, new CartItemRequestDTO { ProductId = pid, Quantity = 6 });

        (await act.Should().ThrowAsync<BadRequestException>())
            .Which.Message.Should().Contain("Not enough stock");
    }

    [Fact]
    public async Task UpdateItemQuantityAsync_WhenCartMissing_ShouldThrow()
    {
        _cartMock.Setup(c => c.GetCartByUserIdAsync(It.IsAny<Guid>())).ReturnsAsync((Cart?)null);

        var act = async () =>
            await _sut.UpdateItemQuantityAsync(Guid.NewGuid(), Guid.NewGuid(), new CartItemUpdateDTO { Quantity = 1 });

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("Cart Not Found");
    }

    [Fact]
    public async Task UpdateItemQuantityAsync_WhenEnoughStock_ShouldUpdate()
    {
        var uid = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var pid = Guid.NewGuid();
        var prod = StockProduct(pid, 5);
        var cartId = Guid.NewGuid();
        var cart = new Cart { Id = cartId, UserId = uid };
        cart.Items.Add(new CartItem { Id = itemId, CartId = cartId, ProductId = pid, Product = prod, Cart = cart });

        _cartMock.SetupSequence(c => c.GetCartByUserIdAsync(uid))
            .ReturnsAsync(() => cart)
            .ReturnsAsync(() => cart);
        _cartMock.Setup(c => c.SaveChangeAsync()).Returns(Task.CompletedTask);

        var dto = await _sut.UpdateItemQuantityAsync(uid, itemId, new CartItemUpdateDTO { Quantity = 3 });

        cart.Items.First(i => i.Id == itemId).Quantity.Should().Be(3);
        dto.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task RemoveItemFromCartAsync_ShouldDelegateAndReload()
    {
        var uid = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var pid = Guid.NewGuid();
        var prod = StockProduct(pid, 10);
        var cartId = Guid.NewGuid();
        var cart = new Cart { Id = cartId, UserId = uid };
        cart.Items.Add(new CartItem { Id = itemId, CartId = cartId, ProductId = pid, Product = prod, Cart = cart });

        _cartMock.SetupSequence(c => c.GetCartByUserIdAsync(uid))
            .ReturnsAsync(() => cart)
            .ReturnsAsync(() => cart);
        _cartMock.Setup(c => c.DeleteItemAsync(itemId)).Returns(Task.CompletedTask);
        _cartMock.Setup(c => c.SaveChangeAsync()).Returns(Task.CompletedTask);

        await _sut.RemoveItemFromCartAsync(uid, itemId);

        _cartMock.Verify(c => c.DeleteItemAsync(itemId), Times.Once);
        _cartMock.Verify(c => c.SaveChangeAsync(), Times.Once);
    }

    [Fact]
    public async Task ClearCartAsync_WhenCartNotFound_ShouldThrow()
    {
        _cartMock.Setup(c => c.GetCartByUserIdAsync(It.IsAny<Guid>())).ReturnsAsync((Cart?)null);

        var act = async () => await _sut.ClearCartAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("Cart not found");
    }

    [Fact]
    public async Task ClearCartAsync_WhenAlreadyEmpty_ShouldThrowBadRequest()
    {
        var uid = Guid.NewGuid();
        var cart = new Cart { Id = Guid.NewGuid(), UserId = uid, Items = new List<CartItem>() };
        _cartMock.Setup(c => c.GetCartByUserIdAsync(uid)).ReturnsAsync(cart);

        var act = async () => await _sut.ClearCartAsync(uid);

        await act.Should().ThrowAsync<BadRequestException>().WithMessage("Cart is already empty");
    }

    [Fact]
    public async Task ClearCartAsync_WhenHasItems_ShouldClear()
    {
        var uid = Guid.NewGuid();
        var cartId = Guid.NewGuid();
        var prod = StockProduct(Guid.NewGuid(), 1);
        var cart = new Cart { Id = cartId, UserId = uid };
        cart.Items.Add(new CartItem
        {
            Id = Guid.NewGuid(),
            CartId = cartId,
            ProductId = prod.Id,
            Product = prod,
            Quantity = 1,
            Cart = cart
        });

        _cartMock.Setup(c => c.GetCartByUserIdAsync(uid)).ReturnsAsync(cart);
        _cartMock.Setup(c => c.ClearCartAsync(cartId)).Returns(Task.CompletedTask);
        _cartMock.Setup(c => c.SaveChangeAsync()).Returns(Task.CompletedTask);

        await _sut.ClearCartAsync(uid);

        _cartMock.Verify(c => c.ClearCartAsync(cartId), Times.Once);
    }
}
