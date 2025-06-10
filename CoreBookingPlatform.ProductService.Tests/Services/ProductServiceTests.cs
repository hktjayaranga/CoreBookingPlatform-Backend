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

namespace CoreBookingPlatform.ProductService.Tests.Services
{
    public class ProductServiceTests : IDisposable
    {
        private readonly ProductDbContext _dbContext;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<CoreBookingPlatform.ProductService.Services.Implementations.ProductService>> _loggerMock;
        private readonly CoreBookingPlatform.ProductService.Services.Implementations.ProductService _service;

        public ProductServiceTests()
        {
            var options = new DbContextOptionsBuilder<ProductDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            _dbContext = new ProductDbContext(options);
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<CoreBookingPlatform.ProductService.Services.Implementations.ProductService>>();
            _service = new CoreBookingPlatform.ProductService.Services.Implementations.ProductService(_dbContext, _mapperMock.Object, _loggerMock.Object);
        }

        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        [Fact]
        public async Task CreateProductAsync_ShouldReturnProductDto_WhenProductCreated()
        {
            // Arrange
            var dto = new CreateProductDto
            {
                ProductName = "Test",
                Currency = "USD",
                Categories = new List<CreateCategoryDto>(),
                Attributes = new List<CreateProductAttributeDto>(),
                Contents = new List<CreateProductContentDto>()
            };
            var product = new Product
            {
                ProductId = 1,
                ProductCategories = new List<ProductCategory>(),
                Attributes = new List<ProductAttribute>(),
                Contents = new List<ProductContent>()
            };

            _mapperMock.Setup(m => m.Map<Product>(dto)).Returns(product);
            _mapperMock.Setup(m => m.Map<ProductDto>(It.IsAny<Product>()))
                .Returns<Product>(p => new ProductDto { ProductId = p.ProductId });

            // Act
            var result = await _service.CreateProductAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.ProductId);
        }

        [Fact]
        public async Task GetAllProductsAsync_ShouldReturnProductDtos()
        {
            // Arrange
            var product = new Product
            {
                ProductId = 1,
                ProductCategories = new List<ProductCategory>(),
                Attributes = new List<ProductAttribute>(),
                Contents = new List<ProductContent>()
            };
            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync();

            _mapperMock.Setup(m => m.Map<IEnumerable<ProductDto>>(It.IsAny<IEnumerable<Product>>()))
                .Returns(new List<ProductDto> { new ProductDto { ProductId = 1 } });

            // Act
            var result = await _service.GetAllProductsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task GetProductByIdAsync_ShouldReturnProductDto_WhenProductExists()
        {
            // Arrange
            var product = new Product
            {
                ProductId = 1,
                ProductCategories = new List<ProductCategory>(),
                Attributes = new List<ProductAttribute>(),
                Contents = new List<ProductContent>()
            };
            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync();

            _mapperMock.Setup(m => m.Map<ProductDto>(It.IsAny<Product>()))
                .Returns(new ProductDto { ProductId = 1 });

            // Act
            var result = await _service.GetProductByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.ProductId);
        }

        [Fact]
        public async Task GetProductByIdAsync_ShouldReturnNull_WhenProductDoesNotExist()
        {
            // Act
            var result = await _service.GetProductByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateProductAsync_ShouldReturnTrue_WhenProductUpdated()
        {
            // Arrange
            // Add a category to the database
            var category = new Category { CategoryId = 1, Name = "TestCat" };
            _dbContext.Categories.Add(category);
            await _dbContext.SaveChangesAsync();

            var product = new Product
            {
                ProductId = 1,
                ProductCategories = new List<ProductCategory>
        {
            new ProductCategory { CategoryId = 1, ProductId = 1 }
        },
                Attributes = new List<ProductAttribute>
        {
            new ProductAttribute { Name = "Color", Value = "Red", ProductId = 1 }
        },
                Contents = new List<ProductContent>
        {
            new ProductContent { ProductId = 1, ContentType = "Image", Title = "Main" }
        }
            };
            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync();

            var updateDto = new UpdateProductDto
            {
                ProductName = "Updated",
                Categories = new List<UpdateCategoryDto>
        {
            new UpdateCategoryDto { Name = "TestCat", Description = "desc" }
        },
                Attributes = new List<UpdateProductAttributeDto>
        {
            new UpdateProductAttributeDto { Name = "Color", Value = "Blue" }
        },
                Contents = new List<UpdateProductContentDto>
        {
            new UpdateProductContentDto { ContentType = "Image", Title = "Main", Description = "desc", MediaUrl = "url", SortOrder = 1 }
        }
            };

            _mapperMock.Setup(m => m.Map(updateDto, product)).Verifiable();

            // Act
            var result = await _service.UpdateProductAsync(1, updateDto);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task UpdateProductAsync_ShouldReturnFalse_WhenProductNotFound()
        {
            // Arrange
            var updateDto = new UpdateProductDto
            {
                ProductName = "Updated",
                Categories = new List<UpdateCategoryDto>(),
                Attributes = new List<UpdateProductAttributeDto>(),
                Contents = new List<UpdateProductContentDto>()
            };

            try
            {
                // Act
                var result = await _service.UpdateProductAsync(999, updateDto);

                // Assert
                Assert.False(result);
            }
            catch (Exception ex)
            {
                Assert.False(true, $"Exception thrown: {ex}");
            }
        }

        [Fact]
        public async Task DeleteProductAsync_ShouldReturnTrue_WhenProductDeleted()
        {
            // Arrange
            var product = new Product
            {
                ProductId = 1,
                ProductCategories = new List<ProductCategory>(),
                Attributes = new List<ProductAttribute>(),
                Contents = new List<ProductContent>()
            };
            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _service.DeleteProductAsync(1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteProductAsync_ShouldReturnFalse_WhenProductNotFound()
        {
            // Act
            var result = await _service.DeleteProductAsync(999);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task CreateProductContentAsync_ShouldReturnProductContentDto_WhenCreated()
        {
            // Arrange
            var dto = new CreateProductContentDto { ProductId = 1, ContentType = "type", Title = "title" };
            var content = new ProductContent
            {
                ProductContentId = 1,
                ProductId = 1
            };
            var contentDto = new ProductContentDto { ProductContentId = 1 };

            _mapperMock.Setup(m => m.Map<ProductContent>(dto)).Returns(content);
            _mapperMock.Setup(m => m.Map<ProductContentDto>(It.IsAny<ProductContent>())).Returns(contentDto);

            // Act
            var result = await _service.CreateProductContentAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.ProductContentId);
        }

        [Fact]
        public async Task GetProductContentByIdAsync_ShouldReturnProductContentDto_WhenExists()
        {
            // Arrange
            var content = new ProductContent
            {
                ProductContentId = 1,
                ProductId = 1
            };
            _dbContext.ProductContent.Add(content);
            await _dbContext.SaveChangesAsync();

            _mapperMock.Setup(m => m.Map<ProductContentDto>(It.IsAny<ProductContent>()))
                .Returns(new ProductContentDto { ProductContentId = 1 });

            // Act
            var result = await _service.GetProductContentByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.ProductContentId);
        }

        [Fact]
        public async Task GetProductContentByIdAsync_ShouldReturnNull_WhenNotFound()
        {
            // Act
            var result = await _service.GetProductContentByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task FindProductContentAsync_ShouldReturnProductContentDto_WhenExists()
        {
            // Arrange
            var content = new ProductContent
            {
                ProductContentId = 1,
                ProductId = 1,
                ContentType = "type",
                Title = "title"
            };
            _dbContext.ProductContent.Add(content);
            await _dbContext.SaveChangesAsync();

            _mapperMock.Setup(m => m.Map<ProductContentDto>(It.IsAny<ProductContent>()))
                .Returns(new ProductContentDto { ProductContentId = 1 });

            // Act
            var result = await _service.FindProductContentAsync(1, "type", "title");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.ProductContentId);
        }

        [Fact]
        public async Task FindProductContentAsync_ShouldReturnNull_WhenNotFound()
        {
            // Act
            var result = await _service.FindProductContentAsync(1, "type", "notfound");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateProductContentAsync_ShouldReturnTrue_WhenUpdated()
        {
            // Arrange
            var content = new ProductContent
            {
                ProductContentId = 1,
                ProductId = 1
            };
            _dbContext.ProductContent.Add(content);
            await _dbContext.SaveChangesAsync();

            var updateDto = new UpdateProductContentDto();

            _mapperMock.Setup(m => m.Map(updateDto, content)).Verifiable();

            // Act
            var result = await _service.UpdateProductContentAsync(1, updateDto);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task UpdateProductContentAsync_ShouldReturnFalse_WhenNotFound()
        {
            // Arrange
            var updateDto = new UpdateProductContentDto();

            // Act
            var result = await _service.UpdateProductContentAsync(999, updateDto);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteProductContentAsync_ShouldReturnTrue_WhenDeleted()
        {
            // Arrange
            var content = new ProductContent
            {
                ProductContentId = 1,
                ProductId = 1
            };
            _dbContext.ProductContent.Add(content);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _service.DeleteProductContentAsync(1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteProductContentAsync_ShouldReturnFalse_WhenNotFound()
        {
            // Act
            var result = await _service.DeleteProductContentAsync(999);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task UpdateProductCoreAsync_ShouldReturnTrue_WhenUpdated()
        {
            // Arrange
            var product = new Product
            {
                ProductId = 1,
                ProductCategories = new List<ProductCategory>(),
                Attributes = new List<ProductAttribute>(),
                Contents = new List<ProductContent>()
            };
            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync();

            var dto = new UpdateProductCoreDto { ProductName = "Core", ProductDescription = "Desc" };

            // Act
            var result = await _service.UpdateProductCoreAsync(1, dto);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task UpdateProductCoreAsync_ShouldReturnFalse_WhenNotFound()
        {
            // Arrange
            var dto = new UpdateProductCoreDto { ProductName = "Core", ProductDescription = "Desc" };

            // Act
            var result = await _service.UpdateProductCoreAsync(999, dto);

            // Assert
            Assert.False(result);
        }
    }
}