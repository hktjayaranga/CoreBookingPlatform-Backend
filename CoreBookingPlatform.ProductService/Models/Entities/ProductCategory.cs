using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoreBookingPlatform.ProductService.Models.Entities
{
    public class ProductCategory
    {
        [Key]
        public int ProductCategoryId { get; set; }
        public int ProductId { get; set; }
        public int CategoryId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        //Navigation
        public Category Category { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}
