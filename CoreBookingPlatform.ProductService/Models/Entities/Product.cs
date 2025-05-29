using System.ComponentModel.DataAnnotations;

namespace CoreBookingPlatform.ProductService.Models.Entities
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        public string? ExternalId { get; set; }

        public string? ProductName { get; set; }

        public string? ProductDescription { get; set; }
        
        public decimal BasePrice { get; set; }

        public string Currency { get; set; } = "USD";

        public string? SKU { get; set; }

        public int? AdapterId { get; set; }
        public string? ExternalSystemName { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

        //Navigation
        
        public ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();

        public ICollection<ProductAttribute> Attributes { get; set; }
        = new List<ProductAttribute>();

        public ICollection<ProductContent> Contents { get; set; } = new List<ProductContent>();
    }
}
