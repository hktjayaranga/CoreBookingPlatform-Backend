using CoreBookingPlatform.ProductService.Controllers;
using CoreBookingPlatform.ProductService.Models.DTOs;
using CoreBookingPlatform.ProductService.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace CoreBookingPlatform.ProductService.Tests.Controllers
{
    public class AttributeControllerTests
    {
        private readonly Mock<IAttributeService> _mockService;
        private readonly Mock<ILogger<AttributeController>> _mockLogger;
        private readonly AttributeController _controller;

        public AttributeControllerTests()
        {
            _mockService = new Mock<IAttributeService>();
            _mockLogger = new Mock<ILogger<AttributeController>>();
            _controller = new AttributeController(_mockService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task CreateAttribute_ReturnsCreated_WhenSuccessful()
        {
            // Arrange
            int productId = 1;
            var createDto = new CreateProductAttributeDto();
            var attributeDto = new ProductAttributeDto { ProductAttributeId = 10 };
            _mockService.Setup(s => s.CreateAttributeAsync(productId, createDto))
                .ReturnsAsync(attributeDto);

            // Act
            var result = await _controller.CreateAttribute(productId, createDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(_controller.GetAttributeById), createdResult.ActionName);
            Assert.Equal(attributeDto, createdResult.Value);
        }

        [Fact]
        public async Task CreateAttribute_ReturnsNotFound_WhenServiceReturnsNull()
        {
            // Arrange
            int productId = 1;
            var createDto = new CreateProductAttributeDto();
            _mockService.Setup(s => s.CreateAttributeAsync(productId, createDto))
                .ReturnsAsync((ProductAttributeDto)null);

            // Act
            var result = await _controller.CreateAttribute(productId, createDto);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CreateAttribute_ReturnsBadRequest_OnException()
        {
            // Arrange
            int productId = 1;
            var createDto = new CreateProductAttributeDto();
            _mockService.Setup(s => s.CreateAttributeAsync(productId, createDto))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.CreateAttribute(productId, createDto);

            // Assert
            Assert.IsType<BadRequestResult>(result.Result);
        }

        [Fact]
        public async Task GetAllAttributes_ReturnsOk_WithAttributes()
        {
            // Arrange
            var attributes = new List<ProductAttributeDto> { new ProductAttributeDto() };
            _mockService.Setup(s => s.GetAllAttributesAsync())
                .ReturnsAsync(attributes);

            // Act
            var result = await _controller.GetAllAttributes();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(attributes, okResult.Value);
        }

        [Fact]
        public async Task GetAllAttributes_ReturnsNotFound_WhenNull()
        {
            // Arrange
            _mockService.Setup(s => s.GetAllAttributesAsync())
                .ReturnsAsync((IEnumerable<ProductAttributeDto>)null);

            // Act
            var result = await _controller.GetAllAttributes();

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetAllAttributes_ReturnsBadRequest_OnException()
        {
            // Arrange
            _mockService.Setup(s => s.GetAllAttributesAsync())
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.GetAllAttributes();

            // Assert
            Assert.IsType<BadRequestResult>(result.Result);
        }

        [Fact]
        public async Task GetAttributeById_ReturnsOk_WhenFound()
        {
            // Arrange
            int id = 5;
            var attribute = new ProductAttributeDto { ProductAttributeId = id };
            _mockService.Setup(s => s.GetAttributeByIdAsync(id))
                .ReturnsAsync(attribute);

            // Act
            var result = await _controller.GetAttributeById(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(attribute, okResult.Value);
        }

        [Fact]
        public async Task GetAttributeById_ReturnsNotFound_WhenNull()
        {
            // Arrange
            int id = 5;
            _mockService.Setup(s => s.GetAttributeByIdAsync(id))
                .ReturnsAsync((ProductAttributeDto)null);

            // Act
            var result = await _controller.GetAttributeById(id);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetAttributeById_ReturnsBadRequest_OnException()
        {
            // Arrange
            int id = 5;
            _mockService.Setup(s => s.GetAttributeByIdAsync(id))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.GetAttributeById(id);

            // Assert
            Assert.IsType<BadRequestResult>(result.Result);
        }

        [Fact]
        public async Task UpdateAttribute_ReturnsNoContent_WhenSuccessful()
        {
            // Arrange
            int id = 2;
            var updateDto = new UpdateProductAttributeDto();
            _mockService.Setup(s => s.UpdateAttributeAsync(id, updateDto))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.UpdateAttribute(id, updateDto);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task UpdateAttribute_ReturnsNotFound_WhenServiceReturnsFalse()
        {
            // Arrange
            int id = 2;
            var updateDto = new UpdateProductAttributeDto();
            _mockService.Setup(s => s.UpdateAttributeAsync(id, updateDto))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.UpdateAttribute(id, updateDto);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UpdateAttribute_ReturnsBadRequest_OnException()
        {
            // Arrange
            int id = 2;
            var updateDto = new UpdateProductAttributeDto();
            _mockService.Setup(s => s.UpdateAttributeAsync(id, updateDto))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.UpdateAttribute(id, updateDto);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Test exception", badRequest.Value);
        }

        [Fact]
        public async Task DeleteAttribute_ReturnsNoContent_WhenSuccessful()
        {
            // Arrange
            int id = 3;
            _mockService.Setup(s => s.DeleteAttributeAsync(id))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteAttribute(id);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteAttribute_ReturnsNotFound_WhenServiceReturnsFalse()
        {
            // Arrange
            int id = 3;
            _mockService.Setup(s => s.DeleteAttributeAsync(id))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteAttribute(id);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteAttribute_ReturnsBadRequest_OnException()
        {
            // Arrange
            int id = 3;
            _mockService.Setup(s => s.DeleteAttributeAsync(id))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.DeleteAttribute(id);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Test exception", badRequest.Value);
        }
    }
}