using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CoreBookingPlatform.ProductService.Data.Context;
using CoreBookingPlatform.ProductService.Models.DTOs;
using CoreBookingPlatform.ProductService.Models.Entities;
using CoreBookingPlatform.ProductService.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class AttributeServiceTests
{
    private readonly Mock<ProductDbContext> _dbContextMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<AttributeService>> _loggerMock;
    private readonly AttributeService _service;

    public AttributeServiceTests()
    {
        var options = new DbContextOptionsBuilder<ProductDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        _dbContextMock = new Mock<ProductDbContext>(options) { CallBase = true };
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<AttributeService>>();
        _service = new AttributeService(_dbContextMock.Object, _mapperMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateAttributeAsync_ShouldCreateAttribute_WhenProductExists()
    {
        // Arrange
        var productId = 1;
        var createDto = new CreateProductAttributeDto { Name = "Color", Value = "Red" };
        var product = new Product { ProductId = productId };
        var attribute = new ProductAttribute { Name = "Color", Value = "Red" };
        var attributeDto = new ProductAttributeDto { Name = "Color", Value = "Red" };

        _dbContextMock.Object.Products.Add(product);
        await _dbContextMock.Object.SaveChangesAsync();

        _mapperMock.Setup(m => m.Map<ProductAttribute>(createDto)).Returns(attribute);
        _mapperMock.Setup(m => m.Map<ProductAttributeDto>(It.IsAny<ProductAttribute>())).Returns(attributeDto);

        // Act
        var result = await _service.CreateAttributeAsync(productId, createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Color", result.Name);
        Assert.Equal("Red", result.Value);
    }

    [Fact]
    public async Task CreateAttributeAsync_ShouldLogWarning_WhenProductDoesNotExist()
    {
        // Arrange
        var productId = 99;
        var createDto = new CreateProductAttributeDto { Name = "Size", Value = "Large" };
        var attribute = new ProductAttribute { Name = "Size", Value = "Large" };
        var attributeDto = new ProductAttributeDto { Name = "Size", Value = "Large" };

        _mapperMock.Setup(m => m.Map<ProductAttribute>(createDto)).Returns(attribute);
        _mapperMock.Setup(m => m.Map<ProductAttributeDto>(It.IsAny<ProductAttribute>())).Returns(attributeDto);

        // Act
        var result = await _service.CreateAttributeAsync(productId, createDto);

        // Assert
        Assert.NotNull(result);
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Product with ID")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAllAttributesAsync_ShouldReturnAllAttributes()
    {
        // Arrange
        var attributes = new List<ProductAttribute>
        {
            new() { ProductAttributeId = 1, Name = "Color", Value = "Red" },
            new() { ProductAttributeId = 2, Name = "Size", Value = "Large" }
        };
        await _dbContextMock.Object.ProductAttributes.AddRangeAsync(attributes);
        await _dbContextMock.Object.SaveChangesAsync();

        _mapperMock.Setup(m => m.Map<IEnumerable<ProductAttributeDto>>(It.IsAny<IEnumerable<ProductAttribute>>()))
            .Returns(new List<ProductAttributeDto>
            {
                new() { ProductAttributeId = 1, Name = "Color", Value = "Red" },
                new() { ProductAttributeId = 2, Name = "Size", Value = "Large" }
            });

        // Act
        var result = await _service.GetAllAttributesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetAttributeByIdAsync_ShouldReturnAttribute_WhenExists()
    {
        // Arrange
        var attribute = new ProductAttribute { ProductAttributeId = 1, Name = "Color", Value = "Red" };
        await _dbContextMock.Object.ProductAttributes.AddAsync(attribute);
        await _dbContextMock.Object.SaveChangesAsync();

        _mapperMock.Setup(m => m.Map<ProductAttributeDto>(attribute))
            .Returns(new ProductAttributeDto { ProductAttributeId = 1, Name = "Color", Value = "Red" });

        // Act
        var result = await _service.GetAttributeByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.ProductAttributeId);
    }

    [Fact]
    public async Task GetAttributeByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Act
        var result = await _service.GetAttributeByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAttributeAsync_ShouldUpdateAndReturnTrue_WhenExists()
    {
        // Arrange
        var attribute = new ProductAttribute { ProductAttributeId = 1, Name = "Color", Value = "Red" };
        await _dbContextMock.Object.ProductAttributes.AddAsync(attribute);
        await _dbContextMock.Object.SaveChangesAsync();

        var updateDto = new UpdateProductAttributeDto { Name = "Color", Value = "Blue" };

        _mapperMock.Setup(m => m.Map(updateDto, attribute)).Callback(() =>
        {
            attribute.Name = updateDto.Name;
            attribute.Value = updateDto.Value;
        });

        // Act
        var result = await _service.UpdateAttributeAsync(1, updateDto);

        // Assert
        Assert.True(result);
        Assert.Equal("Blue", attribute.Value);
    }

    [Fact]
    public async Task UpdateAttributeAsync_ShouldReturnFalse_WhenNotExists()
    {
        // Arrange
        var updateDto = new UpdateProductAttributeDto { Name = "Color", Value = "Blue" };

        // Act
        var result = await _service.UpdateAttributeAsync(999, updateDto);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAttributeAsync_ShouldDeleteAndReturnTrue_WhenExists()
    {
        // Arrange
        var attribute = new ProductAttribute { ProductAttributeId = 1, Name = "Color", Value = "Red" };
        await _dbContextMock.Object.ProductAttributes.AddAsync(attribute);
        await _dbContextMock.Object.SaveChangesAsync();

        // Act
        var result = await _service.DeleteAttributeAsync(1);

        // Assert
        Assert.True(result);
        Assert.Empty(_dbContextMock.Object.ProductAttributes);
    }

    [Fact]
    public async Task DeleteAttributeAsync_ShouldReturnFalse_WhenNotExists()
    {
        // Act
        var result = await _service.DeleteAttributeAsync(999);

        // Assert
        Assert.False(result);
    }
}