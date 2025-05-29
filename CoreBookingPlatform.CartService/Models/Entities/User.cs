namespace CoreBookingPlatform.CartService.Models.Entities
{
    public class User
    {
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; }
        //public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<Cart> Carts { get; set; } = new List<Cart>();
    }
}
