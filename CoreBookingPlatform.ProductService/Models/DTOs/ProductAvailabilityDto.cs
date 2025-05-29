namespace CoreBookingPlatform.ProductService.Models.DTOs
{
    public class ProductAvailabilityDto
    {
        public string ExternalId { get; set; } = null!;
        public bool IsAvailable { get; set; }
        public int Quantity { get; set; }
        public decimal CurrentPrice {  get; set; }
    }
}
