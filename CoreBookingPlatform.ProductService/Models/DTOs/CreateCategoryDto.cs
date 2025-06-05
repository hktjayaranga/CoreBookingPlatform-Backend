using System.ComponentModel.DataAnnotations;

namespace CoreBookingPlatform.ProductService.Models.DTOs
{
    public class CreateCategoryDto
    {
        public int? ProductId { get; set; }
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

    }
}
