using AutoMapper;
using FluentAssertions;
using Moq;
using ShopNN.DTOs;
using ShopNN.Entities;
using ShopNN.Mappings;
using ShopNN.Repositories.Interface;
using ShopNN.Services.Implement;
using ShopNN.Shared.Exeptions;

namespace ShopNN.Tests.Services
{
    public class ProductServiceTests
    {
        private readonly Mock<IProductRepository> _repoMock = new();
        private readonly IMapper _mapper;
        private readonly ProductService _sut;

        public ProductServiceTests()
        {
            var config = new MapperConfiguration(cfg =>
                cfg.AddProfile<MappingProfile>());
            _mapper = config.CreateMapper();

            _sut = new ProductService(_repoMock.Object, _mapper);
        }

        private static Product MakeProduct(Guid? id = null) => new()
        {
            Id          = id ?? Guid.NewGuid(),
            Name        = "Test Product",
            Description = "Description Test Product",
            Price       = 100000,
            Stock       = 10,
            CategoryId  = Guid.NewGuid()
        };

        private static ProductRequestDTO MakeRequest(
            string  name  = "New Product",
            decimal price = 50000,
            int     stock = 5) => new()
        {
            Name        = name,
            Description = "Description New Product",
            Price       = price,
            Stock       = stock
        };
        #region GetAllAsync
        [Fact]
        public async Task GetAllAsync_WhenCalled_ShouldReturnAllProducts()
        {
            var products = new List<Product> {
                MakeProduct(),
                MakeProduct(),
                MakeProduct(),
            };
            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(products);
            var result = await _sut.GetAllAsync();

            result.Should().HaveCount(3);
            _repoMock.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_WhenEmpty_ShouldReturnEmptyList()
        {
            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Product>());

            var result = await _sut.GetAllAsync();

            result.Should().BeEmpty();
            _repoMock.Verify(r => r.GetAllAsync(), Times.Once);
        }
        #endregion
        #region GetByIdAsync
        [Fact]
        public async Task GetByIdAsync_WhenExists_ShouldReturnProduct()
        {
            var id      = Guid.NewGuid();
            var product = MakeProduct(id);
            _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(product);

            var result = await _sut.GetByIdAsync(id);

            result.Should().NotBeNull();
            result.Id.Should().Be(id);
            result.Name.Should().Be(product.Name);
            _repoMock.Verify(r => r.GetByIdAsync(id), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_WhenNotFound_ShouldThrowNotFoundException()
        {
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                     .ReturnsAsync((Product?)null);

            var act = async () => await _sut.GetByIdAsync(Guid.NewGuid());

            await act.Should().ThrowAsync<NotFoundException>()
                     .WithMessage("Product not found");
        }
        #endregion
        #region CreateAsync
        [Fact]
        public async Task CreateAsync_WhenValid_ShouldCreateAndReturnProduct()
        {
            var dto = MakeRequest();
            var product = MakeProduct();
            _repoMock.Setup(r => r.AddAsync(It.IsAny<Product>()))
                     .ReturnsAsync(product); 

            var result = await _sut.CreateAsync(dto);

            result.Should().NotBeNull();
            result.Name.Should().Be(dto.Name);
            result.Price.Should().Be(dto.Price);
            _repoMock.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WhenValid_ShouldGenerateNewId()
        {
            Guid capturedId = Guid.Empty;

            _repoMock.Setup(r => r.AddAsync(It.IsAny<Product>()))
                     .Callback<Product>(p => capturedId = p.Id)
                     .ReturnsAsync((Product p) => p);

            await _sut.CreateAsync(MakeRequest());

            capturedId.Should().NotBe(Guid.Empty);
        }

        [Fact]
        public async Task CreateAsync_WhenValid_ShouldMapDtoFieldsCorrectly()
        {
            var dto = MakeRequest(name: "iPhone 15", price: 999_000, stock: 20);
            Product? captured = null;

            _repoMock.Setup(r => r.AddAsync(It.IsAny<Product>()))
                     .Callback<Product>(p => captured = p)
                     .ReturnsAsync((Product p) => p);

            await _sut.CreateAsync(dto);

            captured!.Name.Should().Be("iPhone 15");
            captured.Price.Should().Be(999_000);
            captured.Stock.Should().Be(20);
        }
        #endregion
        #region UpdateAsync
        [Fact]
        public async Task UpdateAsync_WhenExists_ShouldUpdateAndReturn()
        {
            var id      = Guid.NewGuid();
            var product = MakeProduct(id);
            var dto     = MakeRequest(name: "Updated", price: 200_000);

            _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(product);
            _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Product>()))
                     .Returns(Task.CompletedTask);

            var result = await _sut.UpdateAsync(id, dto);

            result.Name.Should().Be("Updated");
            result.Price.Should().Be(200_000);
            _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenNotFound_ShouldThrowNotFoundException()
        {
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                     .ReturnsAsync((Product?)null);

            var act = async () => await _sut.UpdateAsync(Guid.NewGuid(), MakeRequest());

            await act.Should().ThrowAsync<NotFoundException>()
                     .WithMessage("Product not found");

            _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Never);
        }
        #endregion
        #region DeleteAsync
        [Fact]
        public async Task DeleteAsync_WhenValid_ShouldCallRepositoryAndReturnTrue()
        {
            var id = Guid.NewGuid();
            _repoMock.Setup(r => r.DeleteAsync(id)).Returns(Task.CompletedTask);

            var result = await _sut.DeleteAsync(id);

            result.Should().BeTrue();
            _repoMock.Verify(r => r.DeleteAsync(id), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WhenRepositoryThrows_ShouldPropagate()
        {
            _repoMock.Setup(r => r.DeleteAsync(It.IsAny<Guid>()))
                     .ThrowsAsync(new NotFoundException("Product not found"));

            var act = async () => await _sut.DeleteAsync(Guid.NewGuid());

            await act.Should().ThrowAsync<NotFoundException>()
                     .WithMessage("Product not found");
        }
        #endregion
    }
}
