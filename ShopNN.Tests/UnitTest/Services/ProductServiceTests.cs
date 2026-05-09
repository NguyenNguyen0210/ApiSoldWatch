
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
        // ── Setup chung ──────────────────────────────────────
        private readonly Mock<IProductRepository> _repoMock;
        private readonly IMapper _mapper;
        private readonly ProductService _sut;

        public ProductServiceTests()
        {
            _repoMock = new Mock<IProductRepository>();

            var config = new MapperConfiguration(cfg =>
                cfg.AddProfile<MappingProfile>());
            _mapper = config.CreateMapper();

            _sut = new ProductService(_repoMock.Object, _mapper);
        }

        // ── Helper ───────────────────────────────────────────
        private static Product MakeProduct(Guid? id = null) => new()
        {
            Id = id ?? Guid.NewGuid(),
            Name = "Test Product",
            Description = "Description",
            Price = 100_000,
            Stock = 10
        };

        private static ProductRequestDTO MakeRequest() => new()
        {
            Name = "New Product",
            Description = "Desc",
            Price = 50_000,
            Stock = 5
        };

        // ════════════════════════════════════════════════════
        // GetAllAsync
        // ════════════════════════════════════════════════════
        [Fact]
        public async Task GetAllAsync_ShouldReturnAllProducts()
        {
            // Arrange
            var products = new List<Product> { MakeProduct(), MakeProduct() };
            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(products);

            // Act
            var result = await _sut.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);
            _repoMock.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_WhenEmpty_ShouldReturnEmptyList()
        {
            // Arrange
            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Product>());

            // Act
            var result = await _sut.GetAllAsync();

            // Assert
            result.Should().BeEmpty();
        }

        // ════════════════════════════════════════════════════
        // GetByIdAsync
        // ════════════════════════════════════════════════════
        [Fact]
        public async Task GetByIdAsync_WhenExists_ShouldReturnProduct()
        {
            // Arrange
            var id = Guid.NewGuid();
            var product = MakeProduct(id);
            _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(product);

            // Act
            var result = await _sut.GetByIdAsync(id);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(id);
            result.Name.Should().Be(product.Name);
        }

        [Fact]
        public async Task GetByIdAsync_WhenNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                     .ReturnsAsync((Product?)null);

            // Act
            var act = async () => await _sut.GetByIdAsync(Guid.NewGuid());

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                     .WithMessage("Product not found");
        }

        // ════════════════════════════════════════════════════
        // CreateAsync
        // ════════════════════════════════════════════════════
        [Fact]
        public async Task CreateAsync_ShouldCreateAndReturnProduct()
        {
            // Arrange
            var dto = MakeRequest();
            _repoMock.Setup(r => r.AddAsync(It.IsAny<Product>()))
                     .ReturnsAsync((Product p) => p);

            // Act
            var result = await _sut.CreateAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(dto.Name);
            result.Price.Should().Be(dto.Price);
            _repoMock.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ShouldGenerateNewId()
        {
            // Arrange
            var dto = MakeRequest();
            Guid capturedId = Guid.Empty;

            _repoMock.Setup(r => r.AddAsync(It.IsAny<Product>()))
                     .Callback<Product>(p => capturedId = p.Id)
                     .ReturnsAsync((Product p) => p);

            // Act
            await _sut.CreateAsync(dto);

            // Assert — Id phải được generate, không phải Guid.Empty
            capturedId.Should().NotBe(Guid.Empty);
        }

        // ════════════════════════════════════════════════════
        // UpdateAsync
        // ════════════════════════════════════════════════════
        [Fact]
        public async Task UpdateAsync_WhenExists_ShouldUpdateAndReturn()
        {
            // Arrange
            var id = Guid.NewGuid();
            var product = MakeProduct(id);
            var dto = new ProductRequestDTO
            {
                Name = "Updated",
                Description = "Updated desc",
                Price = 200_000,
                Stock = 3
            };

            _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(product);
            _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Product>()))
                     .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.UpdateAsync(id, dto);

            // Assert
            result.Name.Should().Be("Updated");
            result.Price.Should().Be(200_000);
            _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                     .ReturnsAsync((Product?)null);

            // Act
            var act = async () => await _sut.UpdateAsync(Guid.NewGuid(), MakeRequest());

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                     .WithMessage("Product not found");

            _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Never);
        }

        // ════════════════════════════════════════════════════
        // DeleteAsync
        // ════════════════════════════════════════════════════
        [Fact]
        public async Task DeleteAsync_ShouldCallRepositoryAndReturnTrue()
        {
            // Arrange
            var id = Guid.NewGuid();
            _repoMock.Setup(r => r.DeleteAsync(id)).Returns(Task.CompletedTask);

            // Act
            var result = await _sut.DeleteAsync(id);

            // Assert
            result.Should().BeTrue();
            _repoMock.Verify(r => r.DeleteAsync(id), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WhenRepositoryThrows_ShouldPropagate()
        {
            // Arrange
            _repoMock.Setup(r => r.DeleteAsync(It.IsAny<Guid>()))
                     .ThrowsAsync(new NotFoundException("Product not found"));

            // Act
            var act = async () => await _sut.DeleteAsync(Guid.NewGuid());

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }
    }
}