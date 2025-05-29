using System.ComponentModel.DataAnnotations;

namespace CoreBookingPlatform.ProductService.Models.DTOs
{
    public class UpdateProductDto
    {
        [StringLength(200)]
        public string ProductName { get; set; } = string.Empty;

        public string? ProductDescription { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal BasePrice { get; set; }

        [StringLength(3)]
        public string Currency { get; set; } = "USD";

        [StringLength(50)]
        public string? SKU { get; set; }

        public List<UpdateCategoryDto> Categories { get; set; } = new List<UpdateCategoryDto>();
        public List<UpdateProductAttributeDto> Attributes { get; set; } = new List<UpdateProductAttributeDto>();
        public List<UpdateProductContentDto> Contents { get; set; } = new List<UpdateProductContentDto>();
    }
}
