using Microsoft.Identity.Client;

namespace CoreBookingPlatform.ProductService.Models.DTOs
{
    public class ProductDto
    {
        public int ProductId { get; set; }

        public string? ProductName { get; set; }

        public string? ProductDescription { get; set; }

        public decimal BasePrice { get; set; }

        public string Currency { get; set; } = "USD";

        public string? SKU { get; set; }
        public int AdapterId { get; set; }

        public string? ExternalId { get; set; }
        public string ExternalSystemName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public List<CategoryDto> Categories { get; set; } = new List<CategoryDto>();
        public List<ProductAttributeDto> Attributes { get; set; } = new List<ProductAttributeDto>();
        public List<ProductContentDto> Contents { get; set; } = new List<ProductContentDto>();
    }
}
