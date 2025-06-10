using AutoMapper;
using CoreBookingPlatform.CartService.Data.Context;
using CoreBookingPlatform.CartService.Models.DTOs;
using CoreBookingPlatform.CartService.Models.Entities;
using CoreBookingPlatform.CartService.Services.Implementations;
using CoreBookingPlatform.ProductService.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class CartServiceTests
{
    private CartDbContext GetDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<CartDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        var context = new CartDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    private IMapper GetMapper()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<CoreBookingPlatform.CartService.Mappings.MappingProfile>();
        });
        return config.CreateMapper();
    }

    private Mock<ILogger<CartService>> GetLogger() => new();

    private HttpClient GetMockProductServiceClient(ProductDto product = null, HttpStatusCode status = HttpStatusCode.OK)
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = status,
                Content = new StringContent(product != null ? JsonSerializer.Serialize(product) : "")
            });
        return new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost")
        };
    }

    private IHttpClientFactory GetHttpClientFactory(HttpClient client)
    {
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("ProductService")).Returns(client);
        return factory.Object;
    }

    [Fact]
    public async Task GetCartAsync_ReturnsCart_WhenCartExists()
    {
        // Arrange
        var db = GetDbContext(nameof(GetCartAsync_ReturnsCart_WhenCartExists));
        var user = new User { UserId = "user1", Name = "Test" };
        db.Users.Add(user);
        var cart = new Cart { UserId = "user1", Items = new List<CartItem> { new CartItem { ProductId = 1, Quantity = 2, Price = 10 } } };
        db.Carts.Add(cart);
        db.SaveChanges();
        var service = new CartService(db, GetMapper(), GetLogger().Object, GetHttpClientFactory(GetMockProductServiceClient()));

        // Act
        var result = await service.GetCartAsync("user1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("user1", result.UserId);
        Assert.Single(result.Items);
        Assert.Equal(20, result.TotalPrice);
    }

    [Fact]
    public async Task GetCartAsync_CreatesCart_WhenNotExists()
    {
        var db = GetDbContext(nameof(GetCartAsync_CreatesCart_WhenNotExists));
        db.Users.Add(new User { UserId = "user2", Name = "Test" });
        db.SaveChanges();
        var service = new CartService(db, GetMapper(), GetLogger().Object, GetHttpClientFactory(GetMockProductServiceClient()));

        var result = await service.GetCartAsync("user2");

        Assert.NotNull(result);
        Assert.Equal("user2", result.UserId);
        Assert.Empty(result.Items);
        Assert.True(db.Carts.Any(c => c.UserId == "user2"));
    }

    [Fact]
    public async Task GetCartAsync_Throws_WhenUserIdNull()
    {
        var db = GetDbContext(nameof(GetCartAsync_Throws_WhenUserIdNull));
        var service = new CartService(db, GetMapper(), GetLogger().Object, GetHttpClientFactory(GetMockProductServiceClient()));

        await Assert.ThrowsAsync<ArgumentException>(() => service.GetCartAsync(null));
    }

    [Fact]
    public async Task GetCartAsync_Throws_WhenUserNotFound()
    {
        var db = GetDbContext(nameof(GetCartAsync_Throws_WhenUserNotFound));
        var service = new CartService(db, GetMapper(), GetLogger().Object, GetHttpClientFactory(GetMockProductServiceClient()));

        await Assert.ThrowsAsync<Exception>(() => service.GetCartAsync("nouser"));
    }

    [Fact]
    public async Task AddItemAsync_AddsNewItem_WhenNotExists()
    {
        var db = GetDbContext(nameof(AddItemAsync_AddsNewItem_WhenNotExists));
        db.Users.Add(new User { UserId = "user3", Name = "Test" });
        db.SaveChanges();
        var product = new ProductDto { ProductId = 1, BasePrice = 15 };
        var service = new CartService(db, GetMapper(), GetLogger().Object, GetHttpClientFactory(GetMockProductServiceClient(product)));

        var dto = new CreateCartItemDto { ProductId = 1, Quantity = 2 };
        var result = await service.AddItemAsync("user3", dto);

        Assert.NotNull(result);
        Assert.Equal(1, result.ProductId);
        Assert.Equal(2, result.Quantity);
        Assert.Equal(15, result.Price);
    }

    [Fact]
    public async Task AddItemAsync_IncrementsQuantity_WhenItemExists()
    {
        var db = GetDbContext(nameof(AddItemAsync_IncrementsQuantity_WhenItemExists));
        db.Users.Add(new User { UserId = "user4", Name = "Test" });
        var cart = new Cart { UserId = "user4", Items = new List<CartItem> { new CartItem { ProductId = 2, Quantity = 1, Price = 5 } } };
        db.Carts.Add(cart);
        db.SaveChanges();
        var product = new ProductDto { ProductId = 2, BasePrice = 5 };
        var service = new CartService(db, GetMapper(), GetLogger().Object, GetHttpClientFactory(GetMockProductServiceClient(product)));

        var dto = new CreateCartItemDto { ProductId = 2, Quantity = 3 };
        var result = await service.AddItemAsync("user4", dto);

        Assert.NotNull(result);
        Assert.Equal(2, result.ProductId);
        Assert.Equal(4, result.Quantity);
    }

    [Fact]
    public async Task AddItemAsync_Throws_WhenProductNotFound()
    {
        var db = GetDbContext(nameof(AddItemAsync_Throws_WhenProductNotFound));
        db.Users.Add(new User { UserId = "user5", Name = "Test" });
        db.SaveChanges();
        var service = new CartService(db, GetMapper(), GetLogger().Object, GetHttpClientFactory(GetMockProductServiceClient(null, HttpStatusCode.NotFound)));

        var dto = new CreateCartItemDto { ProductId = 99, Quantity = 1 };
        await Assert.ThrowsAsync<Exception>(() => service.AddItemAsync("user5", dto));
    }

    [Fact]
    public async Task UpdateItemAsync_UpdatesQuantity_WhenItemExists()
    {
        var db = GetDbContext(nameof(UpdateItemAsync_UpdatesQuantity_WhenItemExists));
        db.Users.Add(new User { UserId = "user6", Name = "Test" });
        var cart = new Cart { UserId = "user6", Items = new List<CartItem> { new CartItem { CartItemId = 10, ProductId = 3, Quantity = 1, Price = 7 } } };
        db.Carts.Add(cart);
        db.SaveChanges();
        var service = new CartService(db, GetMapper(), GetLogger().Object, GetHttpClientFactory(GetMockProductServiceClient()));

        var dto = new UpdateCartItemDto { Quantity = 5 };
        var result = await service.UpdateItemAsync("user6", 10, dto);

        Assert.True(result);
        Assert.Equal(5, db.Carts.Include(c => c.Items).First().Items.First().Quantity);
    }

    [Fact]
    public async Task UpdateItemAsync_ReturnsFalse_WhenCartNotFound()
    {
        var db = GetDbContext(nameof(UpdateItemAsync_ReturnsFalse_WhenCartNotFound));
        db.Users.Add(new User { UserId = "user7", Name = "Test" });
        db.SaveChanges();
        var service = new CartService(db, GetMapper(), GetLogger().Object, GetHttpClientFactory(GetMockProductServiceClient()));

        var dto = new UpdateCartItemDto { Quantity = 2 };
        var result = await service.UpdateItemAsync("user7", 1, dto);

        Assert.False(result);
    }

    [Fact]
    public async Task UpdateItemAsync_ReturnsFalse_WhenItemNotFound()
    {
        var db = GetDbContext(nameof(UpdateItemAsync_ReturnsFalse_WhenItemNotFound));
        db.Users.Add(new User { UserId = "user8", Name = "Test" });
        var cart = new Cart { UserId = "user8", Items = new List<CartItem>() };
        db.Carts.Add(cart);
        db.SaveChanges();
        var service = new CartService(db, GetMapper(), GetLogger().Object, GetHttpClientFactory(GetMockProductServiceClient()));

        var dto = new UpdateCartItemDto { Quantity = 2 };
        var result = await service.UpdateItemAsync("user8", 999, dto);

        Assert.False(result);
    }

    [Fact]
    public async Task RemoveItemAsync_RemovesItem_WhenExists()
    {
        var db = GetDbContext(nameof(RemoveItemAsync_RemovesItem_WhenExists));
        db.Users.Add(new User { UserId = "user9", Name = "Test" });
        var cart = new Cart { UserId = "user9", Items = new List<CartItem> { new CartItem { CartItemId = 20, ProductId = 4, Quantity = 1, Price = 8 } } };
        db.Carts.Add(cart);
        db.SaveChanges();
        var service = new CartService(db, GetMapper(), GetLogger().Object, GetHttpClientFactory(GetMockProductServiceClient()));

        var result = await service.RemoveItemAsync("user9", 20);

        Assert.True(result);
        Assert.Empty(db.Carts.Include(c => c.Items).First().Items);
    }

    [Fact]
    public async Task RemoveItemAsync_ReturnsFalse_WhenCartNotFound()
    {
        var db = GetDbContext(nameof(RemoveItemAsync_ReturnsFalse_WhenCartNotFound));
        db.Users.Add(new User { UserId = "user10", Name = "Test" });
        db.SaveChanges();
        var service = new CartService(db, GetMapper(), GetLogger().Object, GetHttpClientFactory(GetMockProductServiceClient()));

        var result = await service.RemoveItemAsync("user10", 1);

        Assert.False(result);
    }

    [Fact]
    public async Task RemoveItemAsync_ReturnsFalse_WhenItemNotFound()
    {
        var db = GetDbContext(nameof(RemoveItemAsync_ReturnsFalse_WhenItemNotFound));
        db.Users.Add(new User { UserId = "user11", Name = "Test" });
        var cart = new Cart { UserId = "user11", Items = new List<CartItem>() };
        db.Carts.Add(cart);
        db.SaveChanges();
        var service = new CartService(db, GetMapper(), GetLogger().Object, GetHttpClientFactory(GetMockProductServiceClient()));

        var result = await service.RemoveItemAsync("user11", 999);

        Assert.False(result);
    }

    [Fact]
    public async Task ClearCartAsync_RemovesAllItems()
    {
        var db = GetDbContext(nameof(ClearCartAsync_RemovesAllItems));
        db.Users.Add(new User { UserId = "user12", Name = "Test" });
        var cart = new Cart { UserId = "user12", Items = new List<CartItem> { new CartItem { ProductId = 5, Quantity = 1, Price = 9 } } };
        db.Carts.Add(cart);
        db.SaveChanges();
        var service = new CartService(db, GetMapper(), GetLogger().Object, GetHttpClientFactory(GetMockProductServiceClient()));

        var result = await service.ClearCartAsync("user12");

        Assert.True(result);
        Assert.Empty(db.Carts.Include(c => c.Items).First().Items);
    }

    [Fact]
    public async Task ClearCartAsync_ReturnsFalse_WhenCartNotFound()
    {
        var db = GetDbContext(nameof(ClearCartAsync_ReturnsFalse_WhenCartNotFound));
        db.Users.Add(new User { UserId = "user13", Name = "Test" });
        db.SaveChanges();
        var service = new CartService(db, GetMapper(), GetLogger().Object, GetHttpClientFactory(GetMockProductServiceClient()));

        var result = await service.ClearCartAsync("user13");

        Assert.False(result);
    }
}