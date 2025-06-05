
using Microsoft.AspNetCore.Mvc;
using System.Collections.Frozen;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CoreBookingPlatform.MockExternalApis.Controllers
{
    [Route("/api/abc")]
    [ApiController]
    public class AbcMockApiController : ControllerBase
    {
        private readonly ILogger<AbcMockApiController> _logger;
        private readonly string _mockDataPath;

        public AbcMockApiController(ILogger<AbcMockApiController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _mockDataPath = configuration["MockDataPath"] ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MockData");
        }

        [HttpGet("Products")]
        public async Task<IActionResult> GetProduct()
        {
            try
            {
                _logger.LogInformation("ABC Mock API : Importing all products");
                var filePath = Path.Combine(_mockDataPath, "abc-products.json");
                if (!System.IO.File.Exists(filePath))
                {
                    _logger.LogWarning("Product data file not found");
                    return NotFound("Mock data file not found");
                }

                var jsonData = await System.IO.File.ReadAllTextAsync(filePath);
                var products = JsonSerializer.Deserialize<List<AbcProductDto>>(jsonData);

                _logger.LogInformation("Returining {Count} products", products?.Count ?? 0);
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("products/{productId}/content")]
        public async Task<IActionResult> GetProductContent(string productId)
        {
            try
            {
                var filePath = Path.Combine(_mockDataPath, "abc-product-content.json");


                if (!System.IO.File.Exists(filePath))
                {
                    _logger.LogWarning("Mock data file not found at {FilePath}", filePath);
                    return NotFound("Mock data file not found");
                }

                var jsonData = await System.IO.File.ReadAllTextAsync(filePath);

                var allContent = JsonSerializer.Deserialize<List<AbcContentDto>>(jsonData);
                if (allContent == null)
                {
                    _logger.LogWarning("Deserialized content list is null");
                    return NotFound("No content available");
                }

                var normalizedId = productId.Trim();
                var contentForThisProduct = allContent
                    .Where(c => c.ProductId?.Trim().Equals(normalizedId, StringComparison.OrdinalIgnoreCase) == true)
                    .ToList();

                if (!contentForThisProduct.Any())
                {
                    return NotFound($"No content for product {productId}");
                }

                return Ok(contentForThisProduct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetProductContent for {ProductId}", productId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("products/{productId}/availability")]
        public async Task<IActionResult> GetProductAvailability(string productId)
        {
            try
            {
                var filePath = Path.Combine(_mockDataPath, "abc-product-availability.json");
                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("Availability data not found");
                }
                var jsonData = await System.IO.File.ReadAllTextAsync(filePath);
                var allAvailabilities = JsonSerializer.Deserialize<List<AbcAvailabilityDto>>(jsonData);
                var availability = allAvailabilities.FirstOrDefault(a => a.ProductId == productId);
                if (availability == null)
                {
                    return NotFound($"No availability data for product {productId}");
                }
                return Ok(availability);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting availability for product {ProductId}", productId);
                return StatusCode(500, "Internal server error");
            }

        }

        #region DTO Classes
        public class AbcProductDto
        {
            public string Id { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public decimal Price { get; set; }
            public string Currency { get; set; } = "USD";
            public string SKU { get; set; } = string.Empty;
            public Dictionary<string, string> Categories { get; set; } = new Dictionary<string, string>();
            public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();


        }
        public class AbcContentDto
        {
            [JsonPropertyName("productId")]
            public string ProductId { get; set; } = string.Empty;

            [JsonPropertyName("type")]
            public string Type { get; set; } = string.Empty;

            [JsonPropertyName("title")]
            public string Title { get; set; } = string.Empty;

            [JsonPropertyName("description")]
            public string? Description { get; set; }

            [JsonPropertyName("mediaUrl")]
            public string? MediaUrl { get; set; }

            [JsonPropertyName("order")]
            public int Order { get; set; }
        }
        public class AbcAvailabilityDto
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