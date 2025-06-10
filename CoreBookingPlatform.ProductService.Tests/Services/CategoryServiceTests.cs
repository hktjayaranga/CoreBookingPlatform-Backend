using AutoMapper;
using CoreBookingPlatform.ProductService.Data.Context;
using CoreBookingPlatform.ProductService.Models.DTOs;
using CoreBookingPlatform.ProductService.Models.Entities;
using CoreBookingPlatform.ProductService.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

public class CategoryServiceTests
{
    private readonly Mock<ProductDbContext> _dbContextMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<CategoryService>> _loggerMock;
    private readonly CategoryService _service;

    public CategoryServiceTests()
    {
        var options = new DbContextOptionsBuilder<ProductDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        _dbContextMock = new Mock<ProductDbContext>(options) { CallBase = true };
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<CategoryService>>();
        _service = new CategoryService(_dbContextMock.Object, _mapperMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateCategoryAsync_ShouldCreateCategory_WhenValid()
    {
        // Arrange
        var createDto = new CreateCategoryDto { Name = "Test", Description = "Desc" };
        var category = new Category { CategoryId = 1, Name = "Test", Description = "Desc" };
        var categoryDto = new CategoryDto { CategoryId = 1, Name = "Test", Description = "Desc" };

        _dbContextMock.Object.Categories.AddRange();
        _mapperMock.Setup(m => m.Map<Category>(createDto)).Returns(category);
        _mapperMock.Setup(m => m.Map<CategoryDto>(category)).Returns(categoryDto);

        // Act
        var result = await _service.CreateCategoryAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test", result.Name);
    }

    [Fact]
    public async Task CreateCategoryAsync_ShouldThrow_WhenCategoryNameExists()
    {
        // Arrange
        var createDto = new CreateCategoryDto { Name = "Duplicate" };
        _dbContextMock.Object.Categories.Add(new Category { Name = "Duplicate" });
        await _dbContextMock.Object.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateCategoryAsync(createDto));
    }

    [Fact]
    public async Task GetAllCategoriesAsync_ShouldReturnAllCategories()
    {
        // Arrange
        var categories = new List<Category>
        {
            new Category { CategoryId = 1, Name = "A" },
            new Category { CategoryId = 2, Name = "B" }
        };
        await _dbContextMock.Object.Categories.AddRangeAsync(categories);
        await _dbContextMock.Object.SaveChangesAsync();

        _mapperMock.Setup(m => m.Map<IEnumerable<CategoryDto>>(It.IsAny<IEnumerable<Category>>()))
            .Returns(categories.Select(c => new CategoryDto { CategoryId = c.CategoryId, Name = c.Name }));

        // Act
        var result = await _service.GetAllCategoriesAsync();

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetCategoryByIdAsync_ShouldReturnCategory_WhenExists()
    {
        // Arrange
        var category = new Category { CategoryId = 1, Name = "Test" };
        await _dbContextMock.Object.Categories.AddAsync(category);
        await _dbContextMock.Object.SaveChangesAsync();

        _mapperMock.Setup(m => m.Map<CategoryDto>(category))
            .Returns(new CategoryDto { CategoryId = 1, Name = "Test" });

        // Act
        var result = await _service.GetCategoryByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.CategoryId);
    }

    [Fact]
    public async Task GetCategoryByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Act
        var result = await _service.GetCategoryByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateCategoryAsync_ShouldUpdate_WhenValid()
    {
        // Arrange
        var category = new Category { CategoryId = 1, Name = "Old" };
        await _dbContextMock.Object.Categories.AddAsync(category);
        await _dbContextMock.Object.SaveChangesAsync();

        var updateDto = new UpdateCategoryDto { Name = "New" };
        _mapperMock.Setup(m => m.Map(updateDto, category)).Callback(() => category.Name = "New");

        // Act
        var result = await _service.UpdateCategoryAsync(1, updateDto);

        // Assert
        Assert.True(result);
        Assert.Equal("New", category.Name);
    }

    [Fact]
    public async Task UpdateCategoryAsync_ShouldReturnFalse_WhenNotFound()
    {
        // Arrange
        var updateDto = new UpdateCategoryDto { Name = "DoesNotExist" };

        // Act
        var result = await _service.UpdateCategoryAsync(999, updateDto);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateCategoryAsync_ShouldThrow_WhenDuplicateName()
    {
        // Arrange
        var category1 = new Category { CategoryId = 1, Name = "A" };
        var category2 = new Category { CategoryId = 2, Name = "B" };
        await _dbContextMock.Object.Categories.AddRangeAsync(category1, category2);
        await _dbContextMock.Object.SaveChangesAsync();

        var updateDto = new UpdateCategoryDto { Name = "B" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateCategoryAsync(1, updateDto));
    }

    [Fact]
    public async Task DeleteCategoryAsync_ShouldDelete_WhenExists()
    {
        // Arrange
        var category = new Category { CategoryId = 1, Name = "ToDelete", ProductCategories = new List<ProductCategory>() };
        await _dbContextMock.Object.Categories.AddAsync(category);
        await _dbContextMock.Object.SaveChangesAsync();

        // Act
        var result = await _service.DeleteCategoryAsync(1);

        // Assert
        Assert.True(result);
        Assert.Null(await _dbContextMock.Object.Categories.FindAsync(1));
    }

    [Fact]
    public async Task DeleteCategoryAsync_ShouldReturnFalse_WhenNotFound()
    {
        // Act
        var result = await _service.DeleteCategoryAsync(999);

        // Assert
        Assert.False(result);
    }
}