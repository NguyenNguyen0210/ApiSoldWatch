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
    private readonly Mock<ICartRepository>    _cartMock    = new();
    private readonly Mock<IProductRepository> _productMock = new();
    private readonly IMapper _mapper;
    private readonly CartService _sut;

    public CartServiceTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();
        _sut = new CartService(_cartMock.Object, _mapper, _productMock.Object);
    }

    private static Product MakeProduct(Guid? id = null, int stock = 10) => new()
    {
        Id          = id ?? Guid.NewGuid(),
        Name        = "Product",
        Description = "Desc",
        Price       = 100,
        Stock       = stock,
        CategoryId  = Guid.NewGuid()
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
    #region GetCartByIdAsync
    [Fact]
    public async Task GetCartByIdAsync_WhenNotFound_ShouldThrowNotFoundException()
    {
        _cartMock.Setup(c => c.GetByIdAsync(It.IsAny<Guid>()))
                 .ReturnsAsync((Cart?)null);

        var act = async () => await _sut.GetCartByIdAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
        _cartMock.Verify(c => c.GetByIdAsync(It.IsAny<Guid>()), Times.Once);
    }

    [Fact]
    public async Task GetCartByIdAsync_WhenFound_ShouldReturnDto()
    {
        var cart = MakeCart();
        _cartMock.Setup(c => c.GetByIdAsync(cart.Id)).ReturnsAsync(cart);

        var result = await _sut.GetCartByIdAsync(cart.Id);

        result.Id.Should().Be(cart.Id);
        _cartMock.Verify(c => c.GetByIdAsync(cart.Id), Times.Once);
    }

    [Fact]
    public async Task GetCartByUserIdAsync_WhenExists_ShouldReturnDtoWithoutCreating()
    {
        var uid  = Guid.NewGuid();
        var cart = MakeCart(uid);
        _cartMock.Setup(c => c.GetCartByUserIdAsync(uid)).ReturnsAsync(cart);

        var result = await _sut.GetCartByUserIdAsync(uid);

        result.Id.Should().Be(cart.Id);
        _cartMock.Verify(c => c.AddAsync(It.IsAny<Cart>()), Times.Never);
    }

    [Fact]
    public async Task GetCartByUserIdAsync_WhenNotExists_ShouldCreateAndPersistNewCart()
    {
        var uid = Guid.NewGuid();
        Cart? persisted = null;

        _cartMock.Setup(c => c.GetCartByUserIdAsync(uid))
                 .ReturnsAsync((Cart?)null);
        _cartMock.Setup(c => c.AddAsync(It.IsAny<Cart>()))
                 .Callback<Cart>(c => persisted = c)
                 .ReturnsAsync((Cart c) => c);

        await _sut.GetCartByUserIdAsync(uid);

        persisted.Should().NotBeNull();
        persisted!.UserId.Should().Be(uid);
        _cartMock.Verify(c => c.AddAsync(It.Is<Cart>(x => x.UserId == uid)), Times.Once);
    }
    #endregion

    #region AddItemToCartAsync
    [Fact]
    public async Task AddItemToCartAsync_WhenQuantityZero_ShouldThrowBadRequestException()
    {
        var dto = new CartItemRequestDTO { ProductId = Guid.NewGuid(), Quantity = 0 };

        var act = async () => await _sut.AddItemToCartAsync(Guid.NewGuid(), dto);

        await act.Should().ThrowAsync<BadRequestException>()
                 .WithMessage("Quantity must be greater than 0");
    }

    [Fact]
    public async Task AddItemToCartAsync_WhenQuantityNegative_ShouldThrowBadRequestException()
    {
        var dto = new CartItemRequestDTO { ProductId = Guid.NewGuid(), Quantity = -1 };

        var act = async () => await _sut.AddItemToCartAsync(Guid.NewGuid(), dto);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task AddItemToCartAsync_WhenProductNotFound_ShouldThrowNotFoundException()
    {
        var dto = new CartItemRequestDTO { ProductId = Guid.NewGuid(), Quantity = 2 };
        _productMock.Setup(p => p.GetByIdAsync(dto.ProductId))
                    .ReturnsAsync((Product?)null);

        var act = async () => await _sut.AddItemToCartAsync(Guid.NewGuid(), dto);

        await act.Should().ThrowAsync<NotFoundException>()
                 .WithMessage("Product not found");
    }

    [Fact]
    public async Task AddItemToCartAsync_WhenStockInsufficient_ShouldThrowBadRequestException()
    {
        var product = MakeProduct(stock: 5);
        var dto     = new CartItemRequestDTO { ProductId = product.Id, Quantity = 20 };
        _productMock.Setup(p => p.GetByIdAsync(product.Id)).ReturnsAsync(product);

        var act = async () => await _sut.AddItemToCartAsync(Guid.NewGuid(), dto);

        await act.Should().ThrowAsync<BadRequestException>()
                 .WithMessage("Not enough stock");
    }

    [Fact]
    public async Task AddItemToCartAsync_WhenNoCart_ShouldCreateCartAndAddItem()
    {
        var uid     = Guid.NewGuid();
        var product = MakeProduct(stock: 10);
        var dto     = new CartItemRequestDTO { ProductId = product.Id, Quantity = 2 };
        Cart? cartRef = null;

        _productMock.Setup(p => p.GetByIdAsync(product.Id)).ReturnsAsync(product);

        _cartMock.SetupSequence(c => c.GetCartByUserIdAsync(uid))
                 .ReturnsAsync((Cart?)null)
                 .ReturnsAsync(() => cartRef);

        _cartMock.Setup(c => c.AddAsync(It.IsAny<Cart>()))
                 .Callback<Cart>(c => { cartRef = c; })
                 .ReturnsAsync((Cart c) => c);

        _cartMock.Setup(c => c.SaveChangeAsync()).Returns(Task.CompletedTask);

        var result = await _sut.AddItemToCartAsync(uid, dto);

        result.Items.Should().HaveCount(1);
        result.Items[0].Quantity.Should().Be(dto.Quantity);
        _cartMock.Verify(c => c.AddAsync(It.IsAny<Cart>()), Times.Once);
        _cartMock.Verify(c => c.SaveChangeAsync(), Times.Once);
    }

    [Fact]
    public async Task AddItemToCartAsync_WhenItemExists_ShouldAccumulateQuantity()
    {
        var uid     = Guid.NewGuid();
        var product = MakeProduct(stock: 10);
        var cart    = MakeCart(uid);
        var item    = MakeCartItem(cart, product, quantity: 3);
        cart.Items.Add(item);

        var dto = new CartItemRequestDTO { ProductId = product.Id, Quantity = 4 };

        _productMock.Setup(p => p.GetByIdAsync(product.Id)).ReturnsAsync(product);
        _cartMock.SetupSequence(c => c.GetCartByUserIdAsync(uid))
                 .ReturnsAsync(cart)
                 .ReturnsAsync(cart);
        _cartMock.Setup(c => c.SaveChangeAsync()).Returns(Task.CompletedTask);

        await _sut.AddItemToCartAsync(uid, dto);

        item.Quantity.Should().Be(7);
        _cartMock.Verify(c => c.SaveChangeAsync(), Times.Once);
    }

    [Fact]
    public async Task AddItemToCartAsync_WhenCombinedExceedsStock_ShouldThrowBadRequestException()
    {
        var uid     = Guid.NewGuid();
        var product = MakeProduct(stock: 10);
        var cart    = MakeCart(uid);
        cart.Items.Add(MakeCartItem(cart, product, quantity: 6));

        _productMock.Setup(p => p.GetByIdAsync(product.Id)).ReturnsAsync(product);
        _cartMock.Setup(c => c.GetCartByUserIdAsync(uid)).ReturnsAsync(cart);

        var act = async () => await _sut.AddItemToCartAsync(uid,
            new CartItemRequestDTO { ProductId = product.Id, Quantity = 6 });

        await act.Should().ThrowAsync<BadRequestException>()
                 .WithMessage("*Not enough stock*");
    }
    #endregion

    #region UpdateItemQuantityAsync
    [Fact]
    public async Task UpdateItemQuantityAsync_WhenQuantityZero_ShouldThrowBadRequestException()
    {
        var act = async () => await _sut.UpdateItemQuantityAsync(
            Guid.NewGuid(), Guid.NewGuid(), new CartItemUpdateDTO { Quantity = 0 });

        await act.Should().ThrowAsync<BadRequestException>()
                 .WithMessage("Quantity must be at least 1");
    }

    [Fact]
    public async Task UpdateItemQuantityAsync_WhenCartNotFound_ShouldThrowNotFoundException()
    {
        _cartMock.Setup(c => c.GetCartByUserIdAsync(It.IsAny<Guid>()))
                 .ReturnsAsync((Cart?)null);

        var act = async () => await _sut.UpdateItemQuantityAsync(
            Guid.NewGuid(), Guid.NewGuid(), new CartItemUpdateDTO { Quantity = 1 });

        await act.Should().ThrowAsync<NotFoundException>()
                 .WithMessage("Cart Not Found");
    }

    [Fact]
    public async Task UpdateItemQuantityAsync_WhenItemNotInCart_ShouldThrowNotFoundException()
    {
        var uid  = Guid.NewGuid();
        var cart = MakeCart(uid); 
        _cartMock.Setup(c => c.GetCartByUserIdAsync(uid)).ReturnsAsync(cart);

        var act = async () => await _sut.UpdateItemQuantityAsync(
            uid, Guid.NewGuid(), new CartItemUpdateDTO { Quantity = 1 });

        await act.Should().ThrowAsync<NotFoundException>()
                 .WithMessage("Cart item not found");
    }

    [Fact]
    public async Task UpdateItemQuantityAsync_WhenStockInsufficient_ShouldThrowBadRequestException()
    {
        var uid     = Guid.NewGuid();
        var product = MakeProduct(stock: 3);
        var cart    = MakeCart(uid);
        var item    = MakeCartItem(cart, product, quantity: 1);
        cart.Items.Add(item);

        _cartMock.Setup(c => c.GetCartByUserIdAsync(uid)).ReturnsAsync(cart);

        var act = async () => await _sut.UpdateItemQuantityAsync(
            uid, item.Id, new CartItemUpdateDTO { Quantity = 10 });

        await act.Should().ThrowAsync<BadRequestException>()
                 .WithMessage("Not enough stock");
    }

    [Fact]
    public async Task UpdateItemQuantityAsync_WhenValid_ShouldUpdateQuantity()
    {
        var uid     = Guid.NewGuid();
        var product = MakeProduct(stock: 10);
        var cart    = MakeCart(uid);
        var item    = MakeCartItem(cart, product, quantity: 1);
        cart.Items.Add(item);

        _cartMock.SetupSequence(c => c.GetCartByUserIdAsync(uid))
                 .ReturnsAsync(cart)
                 .ReturnsAsync(cart);
        _cartMock.Setup(c => c.SaveChangeAsync()).Returns(Task.CompletedTask);

        await _sut.UpdateItemQuantityAsync(uid, item.Id, new CartItemUpdateDTO { Quantity = 5 });

        item.Quantity.Should().Be(5);
        _cartMock.Verify(c => c.SaveChangeAsync(), Times.Once);
    }
    #endregion

    #region RemoveItemFromCartAsync
    [Fact]
    public async Task RemoveItemFromCartAsync_WhenCartNotFound_ShouldThrowNotFoundException()
    {
        _cartMock.Setup(c => c.GetCartByUserIdAsync(It.IsAny<Guid>()))
                 .ReturnsAsync((Cart?)null);

        var act = async () => await _sut.RemoveItemFromCartAsync(Guid.NewGuid(), Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
        _cartMock.Verify(c => c.DeleteItemAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task RemoveItemFromCartAsync_WhenItemNotInCart_ShouldThrowNotFoundException()
    {
        var uid  = Guid.NewGuid();
        var cart = MakeCart(uid);
        _cartMock.Setup(c => c.GetCartByUserIdAsync(uid)).ReturnsAsync(cart);

        var act = async () => await _sut.RemoveItemFromCartAsync(uid, Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
        _cartMock.Verify(c => c.DeleteItemAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task RemoveItemFromCartAsync_WhenValid_ShouldRemoveAndReload()
    {
        var uid     = Guid.NewGuid();
        var product = MakeProduct(stock: 10);
        var cart    = MakeCart(uid);
        var item    = MakeCartItem(cart, product);
        cart.Items.Add(item);

        _cartMock.SetupSequence(c => c.GetCartByUserIdAsync(uid))
                 .ReturnsAsync(cart)
                 .ReturnsAsync(cart);
        _cartMock.Setup(c => c.DeleteItemAsync(item.Id)).Returns(Task.CompletedTask);
        _cartMock.Setup(c => c.UpdateAsync(It.IsAny<Cart>())).Returns(Task.CompletedTask);

        await _sut.RemoveItemFromCartAsync(uid, item.Id);

        _cartMock.Verify(c => c.DeleteItemAsync(item.Id), Times.Once);
    }
    #endregion

    #region ClearCartAsync
    [Fact]
    public async Task ClearCartAsync_WhenCartNotFound_ShouldThrowNotFoundException()
    {
        _cartMock.Setup(c => c.GetCartByUserIdAsync(It.IsAny<Guid>()))
                 .ReturnsAsync((Cart?)null);

        var act = async () => await _sut.ClearCartAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>()
                 .WithMessage("Cart not found");
    }

    [Fact]
    public async Task ClearCartAsync_WhenCartEmpty_ShouldThrowBadRequestException()
    {
        var uid  = Guid.NewGuid();
        var cart = MakeCart(uid);
        _cartMock.Setup(c => c.GetCartByUserIdAsync(uid)).ReturnsAsync(cart);

        var act = async () => await _sut.ClearCartAsync(uid);

        await act.Should().ThrowAsync<BadRequestException>()
                 .WithMessage("Cart is already empty");
    }

    [Fact]
    public async Task ClearCartAsync_WhenHasItems_ShouldClearAndSave()
    {
        var uid     = Guid.NewGuid();
        var product = MakeProduct();
        var cart    = MakeCart(uid);
        cart.Items.Add(MakeCartItem(cart, product));

        _cartMock.Setup(c => c.GetCartByUserIdAsync(uid)).ReturnsAsync(cart);
        _cartMock.Setup(c => c.ClearCartAsync(cart.Id)).Returns(Task.CompletedTask);
        _cartMock.Setup(c => c.SaveChangeAsync()).Returns(Task.CompletedTask);

        await _sut.ClearCartAsync(uid);

        _cartMock.Verify(c => c.ClearCartAsync(cart.Id), Times.Once);
        _cartMock.Verify(c => c.SaveChangeAsync(), Times.Once);
    }
    #endregion
}

