using AutoMapper;
using FluentAssertions;
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
using ShopNN.Shared.Exceptions;
using ShopNN.Shared.Wrappers;

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

            _sut = new ProductService(_repoMock.Object, _mapper, new Mock<ILogger<ProductService>>().Object);
        }

        private static Product MakeProduct(int? id = null) => new()
        {
            Id          = id ?? 1,
            Name        = "Test Product",
            Description = "Description Test Product",
            Price       = 100000,
            Stock       = 10,
            CategoryId  = 1
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
                MakeProduct(1),
                MakeProduct(2),
                MakeProduct(3),
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
            var id      = 1;
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
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                     .ReturnsAsync((Product?)null);

            var act = async () => await _sut.GetByIdAsync(1);

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
            var id      = 1;
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
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                     .ReturnsAsync((Product?)null);

            var act = async () => await _sut.UpdateAsync(1, MakeRequest());

            await act.Should().ThrowAsync<NotFoundException>()
                     .WithMessage("Product not found");

            _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Never);
        }
        #endregion
        #region DeleteAsync
        [Fact]
        public async Task DeleteAsync_WhenValid_ShouldCallRepositoryAndReturnTrue()
        {
            var id = 1;
            _repoMock.Setup(r => r.DeleteAsync(id)).Returns(Task.CompletedTask);

            var result = await _sut.DeleteAsync(id);

            result.Should().BeTrue();
            _repoMock.Verify(r => r.DeleteAsync(id), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WhenRepositoryThrows_ShouldPropagate()
        {
            _repoMock.Setup(r => r.DeleteAsync(It.IsAny<int>()))
                     .ThrowsAsync(new NotFoundException("Product not found"));

            var act = async () => await _sut.DeleteAsync(1);

            await act.Should().ThrowAsync<NotFoundException>()
                     .WithMessage("Product not found");
        }
        #endregion

        #region GetPagedAsync
        private static PagedResult<Product> MakePagedResult(
            List<Product>? items = null, int page = 1, int pageSize = 10, int totalCount = 0)
        {
            var list = items ?? new List<Product>();
            return new PagedResult<Product>
            {
                Items = list,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount == 0 ? list.Count : totalCount
            };
        }

        [Fact]
        public async Task GetPagedAsync_WhenDefaultQuery_ShouldReturnPagedResult()
        {
            var products = new List<Product> { MakeProduct(1), MakeProduct(2), MakeProduct(3) };
            var query = new ProductQueryDTO();

            _repoMock.Setup(r => r.GetPagedAsync(query))
                     .ReturnsAsync(MakePagedResult(products, page: 1, pageSize: 10, totalCount: 3));

            var result = await _sut.GetPagedAsync(query);

            result.Items.Should().HaveCount(3);
            result.Page.Should().Be(1);
            result.PageSize.Should().Be(10);
            result.TotalCount.Should().Be(3);
            result.TotalPages.Should().Be(1);
            result.HasPreviousPage.Should().BeFalse();
            result.HasNextPage.Should().BeFalse();
        }

        [Fact]
        public async Task GetPagedAsync_WhenSearchByName_ShouldPassQueryToRepo()
        {
            var query = new ProductQueryDTO { Search = "rolex" };

            _repoMock.Setup(r => r.GetPagedAsync(It.Is<ProductQueryDTO>(q => q.Search == "rolex")))
                     .ReturnsAsync(MakePagedResult(new List<Product> { MakeProduct(1) }, totalCount: 1));

            var result = await _sut.GetPagedAsync(query);

            result.Items.Should().HaveCount(1);
            _repoMock.Verify(r => r.GetPagedAsync(It.Is<ProductQueryDTO>(q => q.Search == "rolex")), Times.Once);
        }

        [Fact]
        public async Task GetPagedAsync_WhenFilterByCategory_ShouldPassCategoryId()
        {
            var query = new ProductQueryDTO { CategoryId = 2 };

            _repoMock.Setup(r => r.GetPagedAsync(It.Is<ProductQueryDTO>(q => q.CategoryId == 2)))
                     .ReturnsAsync(MakePagedResult(new List<Product> { MakeProduct(1) }, totalCount: 1));

            var result = await _sut.GetPagedAsync(query);

            result.Items.Should().HaveCount(1);
            _repoMock.Verify(r => r.GetPagedAsync(It.Is<ProductQueryDTO>(q => q.CategoryId == 2)), Times.Once);
        }

        [Fact]
        public async Task GetPagedAsync_WhenFilterByPriceRange_ShouldPassMinMaxPrice()
        {
            var query = new ProductQueryDTO { MinPrice = 100, MaxPrice = 500 };

            _repoMock.Setup(r => r.GetPagedAsync(It.Is<ProductQueryDTO>(q => q.MinPrice == 100 && q.MaxPrice == 500)))
                     .ReturnsAsync(MakePagedResult(new List<Product> { MakeProduct(1) }, totalCount: 1));

            var result = await _sut.GetPagedAsync(query);

            result.Items.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetPagedAsync_WhenFilterByInStock_ShouldPassInStockFlag()
        {
            var query = new ProductQueryDTO { InStock = true };

            _repoMock.Setup(r => r.GetPagedAsync(It.Is<ProductQueryDTO>(q => q.InStock == true)))
                     .ReturnsAsync(MakePagedResult(new List<Product> { MakeProduct(1) }, totalCount: 1));

            var result = await _sut.GetPagedAsync(query);

            result.Items.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetPagedAsync_WhenSortByPriceDesc_ShouldPassSortParams()
        {
            var query = new ProductQueryDTO { SortBy = "price", SortOrder = "desc" };

            _repoMock.Setup(r => r.GetPagedAsync(It.Is<ProductQueryDTO>(q => q.SortBy == "price" && q.SortOrder == "desc")))
                     .ReturnsAsync(MakePagedResult(new List<Product> { MakeProduct(1), MakeProduct(2) }, totalCount: 2));

            var result = await _sut.GetPagedAsync(query);

            result.Items.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetPagedAsync_WhenNoResults_ShouldReturnEmptyPagedResult()
        {
            var query = new ProductQueryDTO { Search = "nonexistent" };

            _repoMock.Setup(r => r.GetPagedAsync(query))
                     .ReturnsAsync(MakePagedResult());

            var result = await _sut.GetPagedAsync(query);

            result.Items.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
            result.TotalPages.Should().Be(0);
        }

        [Fact]
        public async Task GetPagedAsync_WhenMultiplePages_ShouldReturnCorrectPaginationInfo()
        {
            var products = new List<Product> { MakeProduct(1), MakeProduct(2) };
            var query = new ProductQueryDTO { Page = 2, PageSize = 2 };

            _repoMock.Setup(r => r.GetPagedAsync(query))
                     .ReturnsAsync(MakePagedResult(products, page: 2, pageSize: 2, totalCount: 5));

            var result = await _sut.GetPagedAsync(query);

            result.Page.Should().Be(2);
            result.PageSize.Should().Be(2);
            result.TotalCount.Should().Be(5);
            result.TotalPages.Should().Be(3);
            result.HasPreviousPage.Should().BeTrue();
            result.HasNextPage.Should().BeTrue();
        }
        #endregion
    }
}

