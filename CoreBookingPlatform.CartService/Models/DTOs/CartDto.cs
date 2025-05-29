namespace CoreBookingPlatform.CartService.Models.DTOs
{
    public class CartDto
    {
        public int CartId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
        public decimal TotalPrice { get; set; }
    }
}
