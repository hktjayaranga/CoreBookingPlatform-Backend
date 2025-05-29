namespace CoreBookingPlatform.ProductService.Models.DTOs
{
    public class UpdateProductCoreDto
    {
        public string? ProductName { get; set; }
        public string? ProductDescription { get; set; }
        public decimal BasePrice { get; set; }
        public string Currency { get; set; } = "USD";
        public string? SKU { get; set; }
    }
}
