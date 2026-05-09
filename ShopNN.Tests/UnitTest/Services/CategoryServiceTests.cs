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

public class CategoryServiceTests
{
    private readonly Mock<ICategoryRespository> _repoMock;
    private readonly IMapper _mapper;
    private readonly CategoryService _sut;

    public CategoryServiceTests()
    {
        _repoMock = new Mock<ICategoryRespository>();
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();
        _sut = new CategoryService(_repoMock.Object, _mapper);
    }

    private static Category MakeCategory(Guid? id = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        Name = "Test Cat"
    };

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllCategories()
    {
        var list = new List<Category> { MakeCategory(), MakeCategory() };
        _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(list);

        var result = await _sut.GetAllAsync();

        result.Should().HaveCount(2);
        _repoMock.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenExists_ShouldReturnCategory()
    {
        var id = Guid.NewGuid();
        var cat = MakeCategory(id);
        _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(cat);

        var result = await _sut.GetByIdAsync(id);

        result.Should().NotBeNull();
        result.Id.Should().Be(id);
        result.Name.Should().Be(cat.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ShouldThrowNotFoundException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Category?)null);

        var act = async () => await _sut.GetByIdAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("Category not found");
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateAndReturnCategory()
    {
        var dto = new CategoryRequestDTO { Name = "Books" };
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Category>())).ReturnsAsync((Category c) => c);

        var result = await _sut.CreateAsync(dto);

        result.Should().NotBeNull();
        result.Name.Should().Be("Books");
        _repoMock.Verify(r => r.AddAsync(It.IsAny<Category>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenExists_ShouldUpdate()
    {
        var id = Guid.NewGuid();
        var entity = MakeCategory(id);
        var dto = new CategoryRequestDTO { Name = "Renamed" };
        _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(entity);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Category>())).Returns(Task.CompletedTask);

        var result = await _sut.UpdateAsync(id, dto);

        result.Name.Should().Be("Renamed");
        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Category>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenNotFound_ShouldThrow()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Category?)null);

        var act = async () => await _sut.UpdateAsync(Guid.NewGuid(), new CategoryRequestDTO { Name = "x" });

        await act.Should().ThrowAsync<NotFoundException>();
        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Category>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ShouldCallRepoAndReturnTrue()
    {
        var id = Guid.NewGuid();
        _repoMock.Setup(r => r.DeleteAsync(id)).Returns(Task.CompletedTask);

        var ok = await _sut.DeleteAsync(id);

        ok.Should().BeTrue();
        _repoMock.Verify(r => r.DeleteAsync(id), Times.Once);
    }
}
