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

        public CdeMockApiController(
            ILogger<CdeMockApiController> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            _mockDataPath = configuration["MockDataPath"] ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MockData");
        }
        [HttpGet("Products")]
        public async Task<IActionResult> GetProduct()
        {
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

        [HttpGet("products/{productId}/content")]
        public async Task<IActionResult> GetProductContent(string productId)
        {
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
                _logger.LogWarning("CDE Mock API: No content for product {ProductId}", productId);
                return NotFound($"No content for product {productId}");
            }

            _logger.LogInformation("CDE Mock API: Returning {Count} content items for product {ProductId}",content.Count, productId);

            return Ok(content);
        }

        [HttpGet("products/{productId}/availability")]
        public async Task<IActionResult> GetProductAvailability(string productId)
        {
            var filePath = Path.Combine(_mockDataPath, "cde-product-availability.json");
            if (!System.IO.File.Exists(filePath))
            {
                _logger.LogWarning("CDE Mock API: Availability data file not found at {Path}", filePath);
                return NotFound("Availability data not found");
            }

            var jsonData = await System.IO.File.ReadAllTextAsync(filePath);
            var allAvail = JsonSerializer.Deserialize<List<CdeAvailabilityDto>>(jsonData);
            var availability = allAvail?
                .FirstOrDefault(a => string.Equals(a.ProductId, productId, StringComparison.OrdinalIgnoreCase));

            if (availability == null)
            {
                _logger.LogWarning("CDE Mock API: No availability for product {ProductId}", productId);
                return NotFound($"No availability data for product {productId}");
            }

            _logger.LogInformation("CDE Mock API: Returning availability for product {ProductId}", productId);
            return Ok(availability);
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
