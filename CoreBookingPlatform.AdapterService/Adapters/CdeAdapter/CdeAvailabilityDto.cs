namespace CoreBookingPlatform.AdapterService.Adapters.CdeAdapter
{
    public class CdeAvailabilityDto
    {
        public string ProductId { get; set; } = "";
        public bool IsAvailable { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
