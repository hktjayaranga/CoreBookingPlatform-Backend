using AutoMapper;
using CoreBookingPlatform.AdapterService.Models.DTOs;
using CoreBookingPlatform.CartService.Models.DTOs;
using CoreBookingPlatform.OrderService.Data.Context;
using CoreBookingPlatform.OrderService.Models.DTOs;
using CoreBookingPlatform.OrderService.Models.Entities;
using CoreBookingPlatform.OrderService.Services.Interfaces;
using CoreBookingPlatform.ProductService.Models.DTOs;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CoreBookingPlatform.OrderService.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly HttpClient _cartServiceClient;
        private readonly HttpClient _adapterServiceClient;
        private readonly HttpClient _productServiceClient;
        private readonly OrderDbContext _orderDbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderService> _logger;

        public OrderService(IHttpClientFactory httpClientFactory, OrderDbContext orderDbContext, IMapper mapper, ILogger<OrderService> logger)
        {
            _cartServiceClient = httpClientFactory.CreateClient("CartService");
            _adapterServiceClient = httpClientFactory.CreateClient("AdapterService");
            _productServiceClient = httpClientFactory.CreateClient("ProductService");
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<OrderDto> CreateOrderAsync(string userId)
        {
            var cartResponse = await _cartServiceClient.GetAsync($"api/cart?userId={userId}");
            if (cartResponse == null)
            {
                throw new Exception("Failed to get cart.");
            }

            var cartJson = await cartResponse.Content.ReadAsStringAsync();
            var cart = JsonSerializer.Deserialize<CartDto>(cartJson);
            if (cart == null)
            {
                throw new Exception("cart is empty");
            }

            //get product details
            var productIds = cart.Items.Select(i => i.ProductId).Distinct().ToList();
            var products = new List<ProductDto>();
            foreach (var productId in productIds)
            {
                var productResponse = await _productServiceClient.GetAsync($"api/products/{productId}");
                if (productResponse == null)
                {
                    throw new Exception($"Product {productId} not found");
                }
                var productJson = await productResponse.Content.ReadAsStringAsync();
                var product = JsonSerializer.Deserialize<ProductDto>(productJson);
                if (product == null)
                {
                    throw new Exception($"Failed to deserialize product {productId}");
                }
                products.Add(product);
            }

            //group items by externa system

            var itemBySystem = products
                .GroupBy(p => p.ExternalSystemName)
                .ToDictionary(
                    g => g.Key,
                    g => cart.Items.Where(i => g.Any(p => p.ProductId == i.ProductId))
                    .Select(i => new BookingItemDto
                    {
                        ExternalProductId = products.First(p => p.ProductId == i.ProductId).ExternalId,
                        Quantity = i.Quantity,
                    }).ToList()
                );

            //availability check
            foreach (var product in products)
            {
                var availabilityResponse = await _adapterServiceClient.GetAsync($"api/availability?externalId={product.ExternalId}&externalSystemName={product.ExternalSystemName}");
                if (availabilityResponse == null)
                {
                    throw new Exception($"Failed checking availability product : {product.ProductId}");
                }
                var availabilityJson = await availabilityResponse.Content.ReadAsStringAsync();
                var availability = JsonSerializer.Deserialize<ProductAvailabilityDto>(availabilityJson);
                if (availability == null)
                {
                    throw new Exception($"Product {product.ProductId} is not available");
                }
                var cartItem = cart.Items.First(i => i.ProductId == product.ProductId);
                if (availability.Quantity < cartItem.Quantity)
                    throw new Exception($"Insufficient quantitiy for product {product.ProductId}");
            }

            var allBookingResults = new List<BookingResultDto>();
            foreach (var system in itemBySystem.Keys)
            {
                var bookingItems = itemBySystem[system];
                var bookingResponse = await _adapterServiceClient.PostAsJsonAsync($"api/bookings?externalSystemName={system}", bookingItems);
                if (bookingResponse == null)
                    throw new Exception("Failed to create booking for {system}");
                var bookingResultJson = await bookingResponse.Content.ReadAsStringAsync();
                var bookingResults = JsonSerializer.Deserialize<List<BookingResultDto>>(bookingResultJson);
                if (bookingResults == null)
                    throw new Exception("Failed to deserialize booking results");
                allBookingResults.AddRange(bookingResults);
            }
            if (allBookingResults.Any(r => !r.Success))
                throw new Exception("Some bookings are failed");

            //create order
            var order = new Order
            {
                UserId = userId,
                Status = "Confirmed",
                TotalPrice = cart.TotalPrice,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Items = cart.Items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    Price = i.Price,
                    ExternalBookingId = allBookingResults.First(r => r.ExternalProductId == products.First(p => p.ProductId == i.ProductId).ExternalId).BookingId
                }).ToList()
            };

            _orderDbContext.Orders.Add(order);
            await _orderDbContext.SaveChangesAsync();

            var clearCart = await _cartServiceClient.DeleteAsync($"api/cart?userId={userId}");
            if (clearCart != null)
                _logger.LogWarning("Faailed to clear cart");
            return _mapper.Map<OrderDto>(order);

        }

        public async Task<OrderDto> GetOrderByIdAsync(int id)
        {
            var order = await _orderDbContext.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.OrderId == id); 
            if (order == null)
            {
                return null;
            }
            else
            {
                return _mapper.Map<OrderDto>(order);
            }
        }

        public async Task<IEnumerable<OrderDto>> GetOrdersByUserAsync(string userId)
        {
            var orders = await _orderDbContext.Orders
                .Include(o => o.Items)
                .Where(o => o.UserId == userId)
                .ToListAsync();
            return _mapper.Map<IEnumerable<OrderDto>>(orders);
            
        }

        public async Task<bool> CancelOrderAsync(int id)
        {
            var order = await _orderDbContext.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.OrderId == id);
            if (order == null || order.Status == "Canceled")
                return false;

            var productIds = order.Items.Select(i => i.ProductId).Distinct().ToList();
            var products = new List<ProductDto>();
            foreach (var productId in productIds)
            {
                var productResponse = await _productServiceClient.GetAsync($"api/products/{productId}");
                if (!productResponse.IsSuccessStatusCode)
                {
                    throw new Exception($"Product {productId} not found");
                }
                var productJson = await productResponse.Content.ReadAsStringAsync();
                var product = JsonSerializer.Deserialize<ProductDto>(productJson);
                if (product == null)
                    throw new Exception($"Failed to deserialize product {productId}.");
                products.Add(product);
            }

            var bookingsBySystem = order.Items
                .GroupBy(i => products.First(p => p.ProductId == i.ProductId).ExternalSystemName ?? "Unknown")
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(i => i.ExternalBookingId).ToList()
                );
            foreach (var system in bookingsBySystem.Keys)
            {
                var bookingIds = bookingsBySystem[system];
                var cancelResponse = await _adapterServiceClient.PostAsJsonAsync(
                    $"api/bookings/cancel?externalSystemName={system}", bookingIds);
                if (!cancelResponse.IsSuccessStatusCode)
                    _logger.LogWarning("Failed to cancel bookings for {System}", system);

            }
            order.Status = "Canceled";
            order.UpdatedAt = DateTime.UtcNow;
            await _orderDbContext.SaveChangesAsync();
            return true;
        }

        

        

        
    }
}
