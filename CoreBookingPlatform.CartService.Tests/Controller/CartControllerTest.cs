using CoreBookingPlatform.CartService.Controllers;
using CoreBookingPlatform.CartService.Models.DTOs;
using CoreBookingPlatform.CartService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace CoreBookingPlatform.CartService.Tests.Controllers
{
    public class CartControllerTests
    {
        private readonly Mock<ICartService> _cartServiceMock;
        private readonly Mock<ILogger<CartController>> _loggerMock;
        private readonly CartController _controller;

        public CartControllerTests()
        {
            _cartServiceMock = new Mock<ICartService>();
            _loggerMock = new Mock<ILogger<CartController>>();
            _controller = new CartController(_cartServiceMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetCart_ReturnsOk_WhenCartExists()
        {
            // Arrange
            var userId = "user1";
            var cartDto = new CartDto { UserId = userId };
            _cartServiceMock.Setup(s => s.GetCartAsync(userId)).ReturnsAsync(cartDto);

            // Act
            var result = await _controller.GetCart(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(cartDto, okResult.Value);
        }

        [Fact]
        public async Task GetCart_ReturnsBadRequest_OnException()
        {
            // Arrange
            var userId = "user1";
            _cartServiceMock.Setup(s => s.GetCartAsync(userId)).ThrowsAsync(new Exception("error"));

            // Act
            var result = await _controller.GetCart(userId);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("error", badRequest.Value);
        }

        [Fact]
        public async Task AddItem_ReturnsCreatedAtAction_WhenSuccess()
        {
            // Arrange
            var userId = "user1";
            var createDto = new CreateCartItemDto { ProductId = 1, Quantity = 2 };
            var itemDto = new CartItemDto { CartItemId = 1, ProductId = 1, Quantity = 2 };
            _cartServiceMock.Setup(s => s.AddItemAsync(userId, createDto)).ReturnsAsync(itemDto);

            // Act
            var result = await _controller.AddItem(userId, createDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(_controller.GetCart), createdResult.ActionName);
            Assert.Equal(itemDto, createdResult.Value);
        }

        [Fact]
        public async Task AddItem_ReturnsBadRequest_OnException()
        {
            // Arrange
            var userId = "user1";
            var createDto = new CreateCartItemDto { ProductId = 1, Quantity = 2 };
            _cartServiceMock.Setup(s => s.AddItemAsync(userId, createDto)).ThrowsAsync(new Exception("fail"));

            // Act
            var result = await _controller.AddItem(userId, createDto);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("fail", badRequest.Value);
        }

        [Fact]
        public async Task RemoveItem_ReturnsNoContent_WhenSuccess()
        {
            // Arrange
            var userId = "user1";
            var itemId = 1;
            _cartServiceMock.Setup(s => s.RemoveItemAsync(userId, itemId)).ReturnsAsync(true);

            // Act
            var result = await _controller.RemoveItem(itemId, userId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task RemoveItem_ReturnsNotFound_WhenNotFound()
        {
            // Arrange
            var userId = "user1";
            var itemId = 1;
            _cartServiceMock.Setup(s => s.RemoveItemAsync(userId, itemId)).ReturnsAsync(false);

            // Act
            var result = await _controller.RemoveItem(itemId, userId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task RemoveItem_ReturnsBadRequest_OnException()
        {
            // Arrange
            var userId = "user1";
            var itemId = 1;
            _cartServiceMock.Setup(s => s.RemoveItemAsync(userId, itemId)).ThrowsAsync(new Exception("remove error"));

            // Act
            var result = await _controller.RemoveItem(itemId, userId);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("remove error", badRequest.Value);
        }

        [Fact]
        public async Task UpdateItem_ReturnsNoContent_WhenSuccess()
        {
            // Arrange
            var userId = "user1";
            var itemId = 1;
            var updateDto = new UpdateCartItemDto { Quantity = 3 };
            _cartServiceMock.Setup(s => s.UpdateItemAsync(userId, itemId, updateDto)).ReturnsAsync(true);

            // Act
            var result = await _controller.UpdateItem(itemId, updateDto, userId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task UpdateItem_ReturnsNotFound_WhenNotFound()
        {
            // Arrange
            var userId = "user1";
            var itemId = 1;
            var updateDto = new UpdateCartItemDto { Quantity = 3 };
            _cartServiceMock.Setup(s => s.UpdateItemAsync(userId, itemId, updateDto)).ReturnsAsync(false);

            // Act
            var result = await _controller.UpdateItem(itemId, updateDto, userId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UpdateItem_ReturnsBadRequest_OnException()
        {
            // Arrange
            var userId = "user1";
            var itemId = 1;
            var updateDto = new UpdateCartItemDto { Quantity = 3 };
            _cartServiceMock.Setup(s => s.UpdateItemAsync(userId, itemId, updateDto)).ThrowsAsync(new Exception("update error"));

            // Act
            var result = await _controller.UpdateItem(itemId, updateDto, userId);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("update error", badRequest.Value);
        }

        [Fact]
        public async Task ClearCart_ReturnsNoContent_WhenSuccess()
        {
            // Arrange
            var userId = "user1";
            _cartServiceMock.Setup(s => s.ClearCartAsync(userId)).ReturnsAsync(true);

            // Act
            var result = await _controller.ClearCart(userId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task ClearCart_ReturnsNotFound_WhenNotFound()
        {
            // Arrange
            var userId = "user1";
            _cartServiceMock.Setup(s => s.ClearCartAsync(userId)).ReturnsAsync(false);

            // Act
            var result = await _controller.ClearCart(userId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task ClearCart_ReturnsBadRequest_OnException()
        {
            // Arrange
            var userId = "user1";
            _cartServiceMock.Setup(s => s.ClearCartAsync(userId)).ThrowsAsync(new Exception("clear error"));

            // Act
            var result = await _controller.ClearCart(userId);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("clear error", badRequest.Value);
        }
    }
}