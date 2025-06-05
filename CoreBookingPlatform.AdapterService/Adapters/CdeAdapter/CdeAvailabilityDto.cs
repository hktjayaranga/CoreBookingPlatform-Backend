using System.Text.Json.Serialization;

namespace CoreBookingPlatform.AdapterService.Adapters.CdeAdapter
{
    public class CdeAvailabilityDto
    {
        [JsonPropertyName("isAvailable")]
        public bool IsAvailable { get; set; }
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }
        [JsonPropertyName("price")]
        public decimal Price { get; set; }
    }
}
