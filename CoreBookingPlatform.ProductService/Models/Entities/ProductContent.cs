using System.ComponentModel.DataAnnotations;

namespace CoreBookingPlatform.ProductService.Models.Entities
{
    public class ProductContent
    {
        [Key]
        public int ProductContentId { get; set; }

        public int ProductId { get; set; }
    
        public string ContentType { get; set; } = string.Empty;

        public string? Title { get; set; }

        public string? Description { get; set; }

        public string? MediaUrl { get; set; }

        public int SortOrder { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt {  get; set; } = DateTime.UtcNow;

        //Navigation
        public Product Product { get; set; } = null!;
    }
}
