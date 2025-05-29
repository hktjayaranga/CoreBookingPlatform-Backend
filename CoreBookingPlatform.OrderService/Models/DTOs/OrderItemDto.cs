namespace CoreBookingPlatform.OrderService.Models.DTOs
{
    public class OrderItemDto
    {
        public int OrderItemId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string ExternalBookingId { get; set; }
    }
}
