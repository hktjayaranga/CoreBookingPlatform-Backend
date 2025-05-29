using System.ComponentModel.DataAnnotations;

namespace CoreBookingPlatform.CartService.Models.Entities
{
    public class Cart
    {
        [Key]
        public int CartId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public User User { get; set; }
        public List<CartItem> Items { get; set; } = new List<CartItem>();

    }
}
