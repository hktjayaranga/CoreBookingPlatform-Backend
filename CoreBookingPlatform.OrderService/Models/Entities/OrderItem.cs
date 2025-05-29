using System.ComponentModel.DataAnnotations;

namespace CoreBookingPlatform.OrderService.Models.Entities
{
    public class OrderItem
    {
        [Key]
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string ExternalBookingId { get; set; }
        public Order Order { get; set; }
    }
}
