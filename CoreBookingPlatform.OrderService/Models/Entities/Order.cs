using System.ComponentModel.DataAnnotations;

namespace CoreBookingPlatform.OrderService.Models.Entities
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }
        public string UserId { get; set; }
        public string Status { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
