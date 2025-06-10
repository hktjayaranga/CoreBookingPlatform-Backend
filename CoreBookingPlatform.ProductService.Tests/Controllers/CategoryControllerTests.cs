using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoreBookingPlatform.ProductService.Controllers;
using CoreBookingPlatform.ProductService.Models.DTOs;
using CoreBookingPlatform.ProductService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CoreBookingPlatform.ProductService.Tests.Controllers
{
    public class CategoryControllerTests
    {
        private readonly Mock<ICategoryService> _mockCategoryService;
        private readonly Mock<ILogger<CategoryController>> _mockLogger;
        private readonly CategoryController _controller;

        public CategoryControllerTests()
        {
            _mockCategoryService = new Mock<ICategoryService>();
            _mockLogger = new Mock<ILogger<CategoryController>>();
            _controller = new CategoryController(_mockCategoryService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task CreateCategory_ReturnsOk_WhenCategoryCreated()
        {
            var createDto = new CreateCategoryDto { Name = "Test" };
            var categoryDto = new CategoryDto { CategoryId = 1, Name = "Test" };
            _mockCategoryService.Setup(s => s.CreateCategoryAsync(createDto)).ReturnsAsync(categoryDto);

            var result = await _controller.CreateCategory(createDto);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(categoryDto, okResult.Value);
        }

        [Fact]
        public async Task CreateCategory_ReturnsNotFound_WhenCategoryIsNull()
        {
            var createDto = new CreateCategoryDto { Name = "Test" };
            _mockCategoryService.Setup(s => s.CreateCategoryAsync(createDto)).ReturnsAsync((CategoryDto)null);

            var result = await _controller.CreateCategory(createDto);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CreateCategory_ReturnsBadRequest_OnException()
        {
            var createDto = new CreateCategoryDto { Name = "Test" };
            _mockCategoryService.Setup(s => s.CreateCategoryAsync(createDto)).ThrowsAsync(new Exception("fail"));

            var result = await _controller.CreateCategory(createDto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("fail", badRequest.Value);
        }

        [Fact]
        public async Task GetAllCategories_ReturnsOk_WhenCategoriesExist()
        {
            var categories = new List<CategoryDto> { new CategoryDto { CategoryId = 1, Name = "Test" } };
            _mockCategoryService.Setup(s => s.GetAllCategoriesAsync()).ReturnsAsync(categories);

            var result = await _controller.GetAllCategories();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(categories, okResult.Value);
        }

        [Fact]
        public async Task GetAllCategories_ReturnsNotFound_WhenCategoriesNull()
        {
            _mockCategoryService.Setup(s => s.GetAllCategoriesAsync()).ReturnsAsync((IEnumerable<CategoryDto>)null);

            var result = await _controller.GetAllCategories();

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetAllCategories_ReturnsBadRequest_OnException()
        {
            _mockCategoryService.Setup(s => s.GetAllCategoriesAsync()).ThrowsAsync(new Exception("fail"));

            var result = await _controller.GetAllCategories();

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("fail", badRequest.Value);
        }

        [Fact]
        public async Task GetCategoryById_ReturnsOk_WhenCategoryExists()
        {
            var category = new CategoryDto { CategoryId = 1, Name = "Test" };
            _mockCategoryService.Setup(s => s.GetCategoryByIdAsync(1)).ReturnsAsync(category);

            var result = await _controller.GetCategoryById(1);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(category, okResult.Value);
        }

        [Fact]
        public async Task GetCategoryById_ReturnsNotFound_WhenCategoryNull()
        {
            _mockCategoryService.Setup(s => s.GetCategoryByIdAsync(1)).ReturnsAsync((CategoryDto)null);

            var result = await _controller.GetCategoryById(1);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetCategoryById_ReturnsBadRequest_OnException()
        {
            _mockCategoryService.Setup(s => s.GetCategoryByIdAsync(1)).ThrowsAsync(new Exception("fail"));

            var result = await _controller.GetCategoryById(1);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("fail", badRequest.Value);
        }

        [Fact]
        public async Task UpdateCategory_ReturnsNoContent_WhenUpdateSucceeds()
        {
            var updateDto = new UpdateCategoryDto { Name = "Updated" };
            _mockCategoryService.Setup(s => s.UpdateCategoryAsync(1, updateDto)).ReturnsAsync(true);

            var result = await _controller.UpdateCategory(1, updateDto);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task UpdateCategory_ReturnsNotFound_WhenUpdateFails()
        {
            var updateDto = new UpdateCategoryDto { Name = "Updated" };
            _mockCategoryService.Setup(s => s.UpdateCategoryAsync(1, updateDto)).ReturnsAsync(false);

            var result = await _controller.UpdateCategory(1, updateDto);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UpdateCategory_ReturnsBadRequest_OnException()
        {
            var updateDto = new UpdateCategoryDto { Name = "Updated" };
            _mockCategoryService.Setup(s => s.UpdateCategoryAsync(1, updateDto)).ThrowsAsync(new Exception("fail"));

            var result = await _controller.UpdateCategory(1, updateDto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("fail", badRequest.Value);
        }

        [Fact]
        public async Task DeleteCategory_ReturnsNoContent_WhenDeleteSucceeds()
        {
            _mockCategoryService.Setup(s => s.DeleteCategoryAsync(1)).ReturnsAsync(true);

            var result = await _controller.DeleteCategory(1);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteCategory_ReturnsNotFound_WhenDeleteFails()
        {
            _mockCategoryService.Setup(s => s.DeleteCategoryAsync(1)).ReturnsAsync(false);

            var result = await _controller.DeleteCategory(1);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteCategory_ReturnsBadRequest_OnException()
        {
            _mockCategoryService.Setup(s => s.DeleteCategoryAsync(1)).ThrowsAsync(new Exception("fail"));

            var result = await _controller.DeleteCategory(1);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("fail", badRequest.Value);
        }
    }
}