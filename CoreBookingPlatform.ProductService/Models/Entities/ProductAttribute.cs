using System.ComponentModel.DataAnnotations;

namespace CoreBookingPlatform.ProductService.Models.Entities
{
    public class ProductAttribute
    {
        [Key]
        public int ProductAttributeId { get; set; }

        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty ;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow ;

        public DateTime UpdatedAt {  get; set; } = DateTime.UtcNow ;

        //Navigation
        public Product Product { get; set; } = null!;
    }
}
