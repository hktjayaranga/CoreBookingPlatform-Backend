using System.ComponentModel.DataAnnotations;

namespace CoreBookingPlatform.ProductService.Models.DTOs
{
    public class CreateProductDto
    {
        [StringLength(200)]
        public string ProductName { get; set; } = string.Empty;

        public string? ProductDescription {  get; set; }

        public decimal BasePrice { get; set; }

        [StringLength(3)]
        public string Currency { get; set; } = "USD";
        
        public string? SKU { get; set; }

        public int? AdapterId { get; set; }

        [StringLength(100)]
        public string? ExternalSystemName { get; set; } = string.Empty;

        public string? ExternalId { get; set; }

        public List<CreateCategoryDto> Categories { get; set; } = new List<CreateCategoryDto>();
        public List<CreateProductAttributeDto> Attributes { get; set; } = new List<CreateProductAttributeDto>();
        public List<CreateProductContentDto> Contents { get; set; } = new List<CreateProductContentDto>();
    }
}
