using CoreBookingPlatform.AdapterService.Constants;
using CoreBookingPlatform.AdapterService.Interfaces;
using CoreBookingPlatform.AdapterService.Models.DTOs;
using CoreBookingPlatform.ProductService.Models.DTOs;
using System.Net;
using System.Text;
using System.Text.Json;

namespace CoreBookingPlatform.AdapterService.Adapters.AbcAdapter
{
    public class AbcAdapter : IAdapter
    {
        private readonly HttpClient _externalApiClient;
        private readonly HttpClient _productServiceClient;
        private readonly ILogger<AbcAdapter> _logger;
        private readonly int _adapterId = 1;
        public string ExternalSystemName => "ABC";

        public AbcAdapter(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<AbcAdapter> logger)
        {

            _externalApiClient = httpClientFactory.CreateClient("AbcApi");
            _productServiceClient = httpClientFactory.CreateClient("ProductApi");

            _logger = logger;
        }
        public async Task ImportProductsAndContentAsync()
        {
            try
            {
                _logger.LogInformation("Fetching products from ABC external API...");
                var response = await _externalApiClient.GetAsync("api/abc/Products");
                response.EnsureSuccessStatusCode();
                var productsJson = await response.Content.ReadAsStringAsync();
                var externalProducts = JsonSerializer.Deserialize<List<AbcProductDto>>(productsJson);

                if (externalProducts == null || !externalProducts.Any())
                {
                    _logger.LogWarning("No products found in ABC external API.");
                    return;
                }

                _logger.LogInformation("Found {Count} products to import.", externalProducts.Count);

                foreach (var externalProduct in externalProducts)
                {
                    if (string.IsNullOrWhiteSpace(externalProduct.Id))
                    {
                        _logger.LogWarning("Skipping product with empty or null ID.");
                        continue;
                    }

                    var existingProductResponse = await _productServiceClient.GetAsync($"api/products/external?externalId={externalProduct.Id}&externalSystemName={ExternalSystemName}");
                    if (existingProductResponse.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("Product {ExternalId} already exists,", externalProduct.Id);
                        continue;
                    }
                    else if (existingProductResponse.StatusCode != HttpStatusCode.NotFound)
                    {
                        _logger.LogWarning("Error checking for existing product {ExternalId}: {StatusCode}",
                            externalProduct.Id, existingProductResponse.StatusCode);
                        continue;
                    }

                    var contentResponse = await _externalApiClient.GetAsync($"api/abc/products/{externalProduct.Id}/content");
                    if (!contentResponse.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("Content not found for product {ExternalId}: {StatusCode}", externalProduct.Id, contentResponse.StatusCode);
                        continue;
                    }

                    var contentJson = await contentResponse.Content.ReadAsStringAsync();
                    var externalContents = JsonSerializer.Deserialize<List<AbcContentDto>>(contentJson) ?? new List<AbcContentDto>();

                    var createProductDto = new CreateProductDto
                    {
                        ProductName = externalProduct.Name,
                        ProductDescription = externalProduct.Description,
                        BasePrice = externalProduct.Price,
                        Currency = externalProduct.Currency,
                        SKU = externalProduct.SKU,
                        AdapterId = _adapterId,
                        ExternalSystemName = ExternalSystemName,
                        ExternalId = externalProduct.Id,
                        Categories = externalProduct.Categories.Select(c => new CreateCategoryDto
                        {
                            Name = c.Key,
                            Description = c.Value
                        }).ToList(),
                        Attributes = externalProduct.Attributes.Select(a => new CreateProductAttributeDto
                        {
                            Name = a.Key,
                            Value = a.Value
                        }).ToList(),
                        Contents = externalContents.Select(content => new CreateProductContentDto
                        {
                            ContentType = content.Type,
                            Title = content.Title,
                            Description = content.Description,
                            MediaUrl = content.MediaUrl,
                            SortOrder = content.Order
                        }).ToList()
                    };

                    var productJson = JsonSerializer.Serialize(createProductDto);
                    var content = new StringContent(productJson, Encoding.UTF8, "application/json");
                    var productResponse = await _productServiceClient.PostAsync("api/products", content);
                    productResponse.EnsureSuccessStatusCode();

                    _logger.LogInformation("Successfully imported product {ExternalId} and its content from ABC.", externalProduct.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing products and content from ABC external API.");
                throw;
            }
        }

        public async Task<ProductAvailabilityDto> CheckAvailabilityAsync(string externalProductId)
        {
            try
            {
                var response = await _externalApiClient.GetAsync($"api/abc/products/{externalProductId}/availability");
                response.EnsureSuccessStatusCode();
                var jsonData = await response.Content.ReadAsStringAsync();
                var availability = JsonSerializer.Deserialize<AbcAvailabilityDto>(jsonData);
                return new ProductAvailabilityDto
                {
                    ExternalId = externalProductId,
                    IsAvailable = availability.IsAvailable,
                    CurrentPrice = availability.Price,
                    Quantity = availability.Quantity
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking availability for product {ExternalId}", externalProductId);
                throw;
            }
        }

        public async Task<List<BookingResultDto>> CreateBookingAsync(List<BookingItemDto> items)
        {
            var results = new List<BookingResultDto>();
            foreach (var item in items)
            {
                try
                {
                    var availabilityResponse = await _externalApiClient.GetAsync($"api/abc/products/{item.ExternalProductId}/availability");
                    availabilityResponse.EnsureSuccessStatusCode();
                    var availabilityJson = await availabilityResponse.Content.ReadAsStringAsync();
                    var availability = JsonSerializer.Deserialize<AbcAvailabilityDto>(availabilityJson);
                    if (availability == null || !availability.IsAvailable || availability.Quantity < item.Quantity)
                    {
                        results.Add(new BookingResultDto
                        {
                            ExternalProductId = item.ExternalProductId,
                            Success = false,
                            ErrorMessage = "Insufficient quantity or not available"
                        });
                        continue;
                    }
                    var bookingId = Guid.NewGuid().ToString(); 
                    results.Add(new BookingResultDto
                    {
                        ExternalProductId = item.ExternalProductId,
                        BookingId = bookingId,
                        Success = true
                    });
                    _logger.LogInformation("Booking created for {ExternalProductId} with BookingId {BookingId}", item.ExternalProductId, bookingId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create booking for {ExternalProductId}", item.ExternalProductId);
                    results.Add(new BookingResultDto
                    {
                        ExternalProductId = item.ExternalProductId,
                        Success = false,
                        ErrorMessage = ex.Message
                    });
                }
            }
            return results;
        }
        public async Task<List<BookingResultDto>> CancelBookingsAsync(List<string> bookingIds)
        {
            var results = new List<BookingResultDto>();
            foreach (var bookingId in bookingIds)
            {
                try
                {
                    _logger.LogInformation("Cancellation for booking {BookingId} in ABC system", bookingId);
                    results.Add(new BookingResultDto
                    {
                        BookingId = bookingId,
                        Success = true
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to cancel booking {BookingId}", bookingId);
                    results.Add(new BookingResultDto
                    {
                        BookingId = bookingId,
                        Success = false,
                        ErrorMessage = ex.Message
                    });
                }
            }
            return results;
        }

        public class AbcBookingResponse
        {
            public string BookingId { get; set; }
        }
    }
}
