using AutoMapper;
using CoreBookingPlatform.AdapterService.Models.DTOs;
using CoreBookingPlatform.CartService.Models.DTOs;
using CoreBookingPlatform.OrderService.Data.Context;
using CoreBookingPlatform.OrderService.Models.DTOs;
using CoreBookingPlatform.OrderService.Models.Entities;
using CoreBookingPlatform.OrderService.Services.Interfaces;
using CoreBookingPlatform.ProductService.Models.DTOs;
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
            _orderDbContext = orderDbContext;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<OrderDto> CreateOrderAsync(string userId)
        {
            try
            {
                ValidateUserId(userId);

                var cartResponse = await _cartServiceClient.GetAsync($"api/cart?userId={userId}");
                if (!cartResponse.IsSuccessStatusCode)
                    throw new Exception($"Failed to get cart: {cartResponse.StatusCode}");

                var cartJson = await cartResponse.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var cart = JsonSerializer.Deserialize<CartDto>(cartJson, options);
                if (cart == null || !cart.Items.Any())
                    throw new Exception("Cannot create order with an empty cart");

                var productIds = cart.Items.Select(i => i.ProductId).Distinct().ToList();
                var products = new List<ProductDto>();
                foreach (var productId in productIds)
                {
                    var productResponse = await _productServiceClient.GetAsync($"api/products/{productId}");
                    if (!productResponse.IsSuccessStatusCode)
                        throw new Exception($"Product {productId} not found");
                    var productJson = await productResponse.Content.ReadAsStringAsync();
                    var product = JsonSerializer.Deserialize<ProductDto>(productJson, options);
                    if (product == null)
                        throw new Exception($"Failed to deserialize product {productId}");
                    products.Add(product);
                }

                foreach (var product in products)
                {
                    var availabilityResponse = await _adapterServiceClient.GetAsync($"api/Import/availability?externalId={product.ExternalId}&externalSystemName={product.ExternalSystemName}");
                    if (!availabilityResponse.IsSuccessStatusCode)
                        throw new Exception($"Failed checking availability for product: {product.ProductId}");
                    var availabilityJson = await availabilityResponse.Content.ReadAsStringAsync();

                    var availability = JsonSerializer.Deserialize<ProductAvailabilityDto>(availabilityJson, options);
                    var cartItem = cart.Items.FirstOrDefault(i => i.ProductId == product.ProductId);
                    if (cartItem == null)
                        throw new Exception($"Cart item for product {product.ProductId} not found");

                    if (availability == null || availability.Quantity < cartItem.Quantity)
                        throw new Exception($"Insufficient quantity for product {product.ProductId}");
                }

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

                var allBookingResults = new List<BookingResultDto>();
                foreach (var system in itemBySystem.Keys)
                {
                    var bookingItems = itemBySystem[system];
                    var bookingResponse = await _adapterServiceClient.PostAsJsonAsync($"api/Import/bookings?externalSystemName={system}", bookingItems);
                    if (!bookingResponse.IsSuccessStatusCode)
                        throw new Exception($"Failed to create booking for {system}");
                    var bookingResultJson = await bookingResponse.Content.ReadAsStringAsync();
                    var bookingResults = JsonSerializer.Deserialize<List<BookingResultDto>>(bookingResultJson, options);
                    if (bookingResults == null)
                        throw new Exception("Failed to deserialize booking results");
                    allBookingResults.AddRange(bookingResults);
                }
                _logger.LogInformation("Booking results: {BookingResults}", JsonSerializer.Serialize(allBookingResults));
                var order = new Order
                {
                    UserId = userId,
                    Status = allBookingResults.All(r => r.Success) ? "Confirmed" : "PartiallyConfirmed",
                    TotalPrice = cart.TotalPrice,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Items = cart.Items.Select(i => new OrderItem
                    {
                        ProductId = i.ProductId,
                        Quantity = i.Quantity,
                        Price = i.Price,
                        ExternalBookingId = allBookingResults.FirstOrDefault(r => r.ExternalProductId == products.First(p => p.ProductId == i.ProductId).ExternalId)?.BookingId ?? "N/A"
                    }).ToList()
                };

                _orderDbContext.Orders.Add(order);
                await _orderDbContext.SaveChangesAsync();

                _logger.LogInformation("Order created successfully with OrderId {OrderId}", order.OrderId);
                var orderDetails = new
                {
                    OrderId = order.OrderId,
                    UserId = order.UserId,
                    Status = order.Status,
                    TotalPrice = order.TotalPrice,
                    Items = order.Items.Select(i => new { i.ProductId, i.Quantity, i.Price, i.ExternalBookingId }),
                    CreatedAt = order.CreatedAt
                };
                _logger.LogInformation("Order Success Message Sent to External System: {OrderDetails}",JsonSerializer.Serialize(orderDetails, new JsonSerializerOptions { WriteIndented = true }));


                var clearCartResponse = await _cartServiceClient.DeleteAsync($"api/cart?userId={userId}");

                return _mapper.Map<OrderDto>(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occrred while creating order for user {UserId}", userId);
                throw;
            }
        }

        public async Task<OrderDto> GetOrderByIdAsync(int id)
        {
            try
            {
                var order = await _orderDbContext.Orders
                    .Include(o => o.Items)
                    .FirstOrDefaultAsync(o => o.OrderId == id);
                if (order == null)
                {
                    return null;
                }
                return _mapper.Map<OrderDto>(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching order with ID {OrderId}", id);
                throw;
            }

        }

        public async Task<IEnumerable<OrderDto>> GetOrdersByUserAsync(string userId)
        {
            try
            {
                var orders = await _orderDbContext.Orders
                    .Include(o => o.Items)
                    .Where(o => o.UserId == userId)
                    .ToListAsync();
                return _mapper.Map<IEnumerable<OrderDto>>(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching orders for user {UserId}", userId);
                throw;
            }

        }

        public async Task<bool> CancelOrderAsync(int id)
        {
            try
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
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var product = JsonSerializer.Deserialize<ProductDto>(productJson, options);
                    if (product == null)
                        throw new Exception($"Failed to deserialize product {productId}.");
                    if (product.ProductId != productId)
                    {
                        _logger.LogError("ProductId mismatch: requested {RequestedId}, received {ReceivedId}", productId, product.ProductId);
                        throw new Exception($"ProductId mismatch: requested {productId}, received {product.ProductId}");
                    }
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
                        $"api/Import/bookings/cancel?externalSystemName={system}", bookingIds);
                    if (!cancelResponse.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("Failed to cancel bookings for {System}", system);
                    }
                    else
                    {
                        var cancelResultsJson = await cancelResponse.Content.ReadAsStringAsync();
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var cancelResults = JsonSerializer.Deserialize<List<BookingResultDto>>(cancelResultsJson, options);
                        _logger.LogInformation("Cancellation results for {System}: {CancelResults}",
                            system, JsonSerializer.Serialize(cancelResults));
                        Console.WriteLine($"Order cancellation response for {system}:");
                        Console.WriteLine(JsonSerializer.Serialize(cancelResults, new JsonSerializerOptions { WriteIndented = true }));
                    }
                }
                order.Status = "Canceled";
                order.UpdatedAt = DateTime.UtcNow;
                await _orderDbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while canceling order with ID {OrderId}", id);
                throw;
            }
        }



        public async Task<AvailabilityCheckResultDto> CheckCartAvailabilityAsync(string userId)
        {
            try
            {
                ValidateUserId(userId);

                var cartResponse = await _cartServiceClient.GetAsync($"api/cart?userId={userId}");
                if (!cartResponse.IsSuccessStatusCode)
                    throw new Exception("Failed to retrieve cart");

                var cartJson = await cartResponse.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var cart = JsonSerializer.Deserialize<CartDto>(cartJson, options);
                if (cart == null || !cart.Items.Any())
                {
                    _logger.LogWarning("Cart for user {UserId} is empty or null", userId);
                    return new AvailabilityCheckResultDto { IsAvailable = false };
                }

                var productIds = cart.Items.Select(i => i.ProductId).Distinct().ToList();
                var products = new List<ProductDto>();
                var unavailableItems = new List<UnavailableItemDto>();
                foreach (var productId in productIds)
                {
                    var productResponse = await _productServiceClient.GetAsync($"api/products/{productId}");
                    if (!productResponse.IsSuccessStatusCode)
                        throw new Exception($"Product {productId} not found");
                    var productJson = await productResponse.Content.ReadAsStringAsync();
                    var product = JsonSerializer.Deserialize<ProductDto>(productJson, options);
                    if (product == null)
                        throw new Exception($"Failed to deserialize product {productId}");
                    products.Add(product);
                }

                foreach (var item in cart.Items)
                {
                    var product = products.FirstOrDefault(p => p.ProductId == item.ProductId);
                    if (product == null)
                    {
                        unavailableItems.Add(new UnavailableItemDto { ProductId = item.ProductId, Reason = "Product not found" });
                        continue;
                    }

                    var availabilityResponse = await _adapterServiceClient.GetAsync($"api/Import/availability?externalId={product.ExternalId}&externalSystemName={product.ExternalSystemName}");
                    if (!availabilityResponse.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("Availability check failed for product {ProductId}: Status={StatusCode}",
                            item.ProductId, availabilityResponse.StatusCode);
                        unavailableItems.Add(new UnavailableItemDto { ProductId = item.ProductId, Reason = "Availability check failed" });
                        continue;
                    }

                    var availabilityJson = await availabilityResponse.Content.ReadAsStringAsync();

                    var availability = JsonSerializer.Deserialize<ProductAvailabilityDto>(availabilityJson, options);
                    if (availability == null)
                    {
                        unavailableItems.Add(new UnavailableItemDto { ProductId = item.ProductId, Reason = "Deserialization failed" });
                        continue;
                    }

                    if (!availability.IsAvailable || availability.Quantity < item.Quantity)
                    {
                        unavailableItems.Add(new UnavailableItemDto { ProductId = item.ProductId, Reason = "Insufficient quantity" });
                    }
                }

                var result = new AvailabilityCheckResultDto
                {
                    IsAvailable = unavailableItems.Count == 0,
                    UnavailableItems = unavailableItems
                };

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while checking cart availability for user {UserId}", userId);
                throw;
            }
        }

        public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
        {
            try
            {
                var orders = await _orderDbContext.Orders
                     .Include(o => o.Items)
                     .ToListAsync();
                return _mapper.Map<IEnumerable<OrderDto>>(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching all orders");
                throw;
            }

        }


        private static void ValidateUserId(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("UserId cannot be null or empty");
        }
    }
}
