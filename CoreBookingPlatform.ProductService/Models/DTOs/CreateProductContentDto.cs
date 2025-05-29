using System.ComponentModel.DataAnnotations;

namespace CoreBookingPlatform.ProductService.Models.DTOs
{
    public class CreateProductContentDto
    {
        public int ProductId { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? MediaUrl { get; set; }
        public int SortOrder { get; set; } = 0;
    }
}
