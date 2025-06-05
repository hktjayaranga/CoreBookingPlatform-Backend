using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CoreBookingPlatform.MockExternalApis.Controllers
{
    [Route("api/cde")]
    [ApiController]
    public class CdeMockApiController : ControllerBase
    {
        private readonly ILogger<CdeMockApiController> _logger;
        private readonly string _mockDataPath;

        public CdeMockApiController(ILogger<CdeMockApiController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _mockDataPath = configuration["MockDataPath"] ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MockData");
        }

        [HttpGet("Products")]
        public async Task<IActionResult> GetProduct()
        {
            try
            {
                _logger.LogInformation("CDE Mock API: Importing all products");
                var filePath = Path.Combine(_mockDataPath, "cde-products.json");
                if (!System.IO.File.Exists(filePath))
                {
                    _logger.LogWarning("CDE Mock API: Product data file not found at {Path}", filePath);
                    return NotFound("Mock data file not found");
                }

                var jsonData = await System.IO.File.ReadAllTextAsync(filePath);
                var products = JsonSerializer.Deserialize<List<CdeProductDto>>(jsonData);

                _logger.LogInformation("CDE Mock API: Returning {Count} products", products?.Count ?? 0);
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CDE Mock API: Error getting products");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("products/{productId}/content")]
        public async Task<IActionResult> GetProductContent(string productId)
        {
            try
            {
                _logger.LogInformation("CDE Mock API: Fetching content for product {ProductId}", productId);
                var filePath = Path.Combine(_mockDataPath, "cde-product-content.json");

                if (!System.IO.File.Exists(filePath))
                {
                    _logger.LogWarning("CDE Mock API: Content data file not found at {Path}", filePath);
                    return NotFound("Mock data file not found");
                }

                var jsonData = await System.IO.File.ReadAllTextAsync(filePath);
                var allContent = JsonSerializer.Deserialize<Dictionary<string, List<CdeContentDto>>>(jsonData);

                if (allContent == null || !allContent.TryGetValue(productId, out var content))
                {
                    _logger.LogWarning("CDE Mock API: No content found for product {ProductId}", productId);
                    return NotFound($"No content for product {productId}");
                }

                _logger.LogInformation("CDE Mock API: Returning {Count} content items for product {ProductId}", content.Count, productId);
                return Ok(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CDE Mock API: Error getting content for product {ProductId}", productId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("products/{productId}/availability")]
        public async Task<IActionResult> GetProductAvailability(string productId)
        {
            try
            {
                _logger.LogInformation("CDE Mock API: Fetching availability for product {ProductId}", productId);
                var filePath = Path.Combine(_mockDataPath, "cde-product-availability.json");

                if (!System.IO.File.Exists(filePath))
                {
                    _logger.LogWarning("CDE Mock API: Availability data file not found at {Path}", filePath);
                    return NotFound("Availability data not found");
                }

                var jsonData = await System.IO.File.ReadAllTextAsync(filePath);
                var allAvailabilities = JsonSerializer.Deserialize<List<CdeAvailabilityDto>>(jsonData);
                var availability = allAvailabilities?
                    .FirstOrDefault(a => string.Equals(a.ProductId, productId, StringComparison.OrdinalIgnoreCase));

                if (availability == null)
                {
                    _logger.LogWarning("CDE Mock API: No availability data found for product {ProductId}", productId);
                    return NotFound($"No availability data for product {productId}");
                }

                _logger.LogInformation("CDE Mock API: Returning availability for product {ProductId}", productId);
                return Ok(availability);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CDE Mock API: Error getting availability for product {ProductId}", productId);
                return StatusCode(500, "Internal server error");
            }
        }

        #region DTOs
        public class CdeProductDto
        {
            public string Id { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public decimal Price { get; set; }
            public string Currency { get; set; } = "USD";
            public string SKU { get; set; } = string.Empty;
            public Dictionary<string, string> Categories { get; set; } = new();
            public Dictionary<string, string> Attributes { get; set; } = new();
        }

        public class CdeContentDto
        {
            public string ProductId { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
            public string Title { get; set; } = string.Empty;
            public string? Description { get; set; }
            public string? MediaUrl { get; set; }
            public int Order { get; set; }
        }

        public class CdeAvailabilityDto
        {
            [JsonPropertyName("productId")]
            public string ProductId { get; set; } = string.Empty;
            [JsonPropertyName("isAvailable")]
            public bool IsAvailable { get; set; }
            [JsonPropertyName("quantity")]
            public int Quantity { get; set; }
            [JsonPropertyName("price")]
            public decimal Price { get; set; }
        }
        #endregion
    }
}