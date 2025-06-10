using CoreBookingPlatform.ProductService.Controllers;
using CoreBookingPlatform.ProductService.Models.DTOs;
using CoreBookingPlatform.ProductService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBookingPlatform.ProductService.Tests.Controllers
{
    public class ProductControllerTests
    {
        private readonly Mock<IProductService> _mockProductService;
        private readonly Mock<ILogger<ProductController>> _mockLogger;
        private readonly ProductController _productController;

        public ProductControllerTests()
        {
            _mockProductService = new Mock<IProductService>();
            _mockLogger = new Mock<ILogger<ProductController>>();
            _productController = new ProductController(_mockProductService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetAllProducts_WhenProductExist_ReturnsOkWithList()
        {
            //Arrange
            var products = new List<ProductDto> { new ProductDto { ProductId = 1, ProductName = "Product 1" } };
            _mockProductService.Setup(s => s.GetAllProductsAsync()).ReturnsAsync(products);

            //Act
            var result = await _productController.GetAllProducts();

            //Assert
            var okObjectResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedProducts = Assert.IsType<List<ProductDto>>(okObjectResult.Value);
            Assert.Single(returnedProducts);
        }

        [Fact]
        public async Task GetAllProducts_WhenNoProductExist_ReturnOkWithEmptyList()
        {
            //Arrange
            _mockProductService.Setup(s => s.GetAllProductsAsync()).ReturnsAsync(new List<ProductDto>());

            //Act
            var result = await _productController.GetAllProducts();

            //Assert
            var okObjectResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedProducts = Assert.IsType<List<ProductDto>>(okObjectResult.Value);
            Assert.Empty(returnedProducts);
        }

        [Fact]
        public async Task GetProductById_ValidId_ReturnsOkWithProduct()
        {
            // Arrange
            var product = new ProductDto { ProductId = 1, ProductName = "Product 1" };
            _mockProductService.Setup(s => s.GetProductByIdAsync(1)).ReturnsAsync(product);

            // Act
            var result = await _productController.GetProductById(1);

            // Assert
            var okObjectResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedProduct = Assert.IsType<ProductDto>(okObjectResult.Value);
            Assert.Equal(200, okObjectResult.StatusCode);
            Assert.Equal(product.ProductId, returnedProduct.ProductId);
        }

        [Fact]
        public async Task GetProductById_InvalidId_ReturnsNotFound()
        {
            // Arrange
            _mockProductService.Setup(s => s.GetProductByIdAsync(999)).ReturnsAsync((ProductDto)null);
            // Act
            var result = await _productController.GetProductById(999);
            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CreateProduct_ValidInput_ReturnsCreated()
        {
            //Arrange
            var createProductDto = new CreateProductDto { ProductName = "New Product" };
            var createdProduct = new ProductDto { ProductId = 1, ProductName = "New Product" };
            _mockProductService.Setup(s => s.CreateProductAsync(createProductDto)).ReturnsAsync(createdProduct);

            //Act
            var result = await _productController.CreateProduct(createProductDto);

            //Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnedProduct = Assert.IsType<ProductDto>(createdResult.Value);
            Assert.Equal("GetProductById", createdResult.ActionName);
            Assert.Equal(1, createdResult.RouteValues["id"]);
            Assert.Equal(createdProduct, createdResult.Value);
        }

        [Fact]
        public async Task CreateProduct_ExistingProduct_ReturnsConflict()
        {
            //Arrange
            var createProductDto = new CreateProductDto { ProductName = "Existing Product", ExternalId = "EXT123", ExternalSystemName = "System1" };
            _mockProductService.Setup(s => s.CreateProductAsync(createProductDto)).ReturnsAsync((ProductDto)null);

            //Act
            var result = await _productController.CreateProduct(createProductDto);

            //Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result.Result);
            Assert.Equal(409, conflictResult.StatusCode);
        }

        [Fact]
        public async Task UpdateProduct_SuccessfulUpdate_ReturnsNoContent()
        {
            // Arrange
            var updateDto = new UpdateProductDto { ProductName = "Updated Product" };
            _mockProductService.Setup(s => s.UpdateProductAsync(1, updateDto)).ReturnsAsync(true);

            // Act
            var result = await _productController.UpdateProduct(1, updateDto);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task UpdateProduct_ProductNotFound_ReturnsNotFound()
        {
            // Arrange
            var updateDto = new UpdateProductDto { ProductName = "Updated Product" };
            _mockProductService.Setup(s => s.UpdateProductAsync(999, updateDto)).ReturnsAsync(false);

            // Act
            var result = await _productController.UpdateProduct(999, updateDto);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteProduct_SuccessfulDeletion_ReturnsNoContent()
        {
            // Arrange
            _mockProductService.Setup(s => s.DeleteProductAsync(1)).ReturnsAsync(true);

            // Act
            var result = await _productController.DeleteProduct(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteProduct_ProductNotFound_ReturnsNotFound()
        {
            // Arrange
            _mockProductService.Setup(s => s.DeleteProductAsync(999)).ReturnsAsync(false);

            // Act
            var result = await _productController.DeleteProduct(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task CreateProductContent_ValidInput_ReturnsCreated()
        {
            //Arrange
            var createContentDto = new CreateProductContentDto
            {
                ProductId = 1,
                ContentType = "Image",
                Title = "Test Image"
            };
            var createdContent = new ProductContentDto
            {
                ProductContentId = 1,
                ProductId = 1,
                ContentType = "Image",
                Title = "Test Image"
            };
            _mockProductService.Setup(s => s.CreateProductContentAsync(createContentDto)).ReturnsAsync(createdContent);

            //Act
            var result = await _productController.CreateProductContent(createContentDto);

            //Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal("GetProductContentById", createdResult.ActionName);
            Assert.Equal(1, createdResult.RouteValues["id"]);
            Assert.Equal(createdContent, createdResult.Value);
        }

        [Fact]
        public async Task CreateProductContent_ServiceThrowsException_ReturnBadRequest()
        {
            // Arrange
            var createContentDto = new CreateProductContentDto
            {
                ProductId = 1,
                ContentType = "Image",
                Title = "Test Image"
            };
            _mockProductService.Setup(s => s.CreateProductContentAsync(createContentDto))
                .ThrowsAsync(new System.Exception("Test exception"));

            // Act
            var result = await _productController.CreateProductContent(createContentDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Test exception", badRequestResult.Value);
        }

        [Fact]
        public async Task GetByExternal_ValidExternal_ReturnsOkWithProduct()
        {
            //Arrange
            var externalId = "EXT1";
            var system = "ABC";
            var matching = new ProductDto { ProductId = 10, ExternalId = externalId, ExternalSystemName = system };
            var list = new List<ProductDto> { new ProductDto { ProductId = 5, ExternalId = "foo", ExternalSystemName = "bar" }, matching };
            _mockProductService.Setup(s => s.GetAllProductsAsync()).ReturnsAsync(list);

            //Act
            var result = await _productController.GetByExternal(externalId, system);

            // Assert
            var okObjectResult = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<ProductDto>(okObjectResult.Value);
            Assert.Equal(matching.ProductId, dto.ProductId);
        }

        [Fact]
        public async Task GetByExternal_NotFound_ReturnsNotFound()
        {
            // Arrange
            _mockProductService.Setup(s => s.GetAllProductsAsync()).ReturnsAsync(new List<ProductDto>());

            // Act
            var result = await _productController.GetByExternal("doesnt", "exist");

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetByExternal_ServiceThrows_ReturnsBadRequest()
        {
            // Arrange
            _mockProductService
              .Setup(s => s.GetAllProductsAsync())
              .ThrowsAsync(new Exception("oops"));

            // Act
            var result = await _productController.GetByExternal("x", "y");

            // Assert
            var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("oops", bad.Value);
        }
    }
}
