using System.Text.Json.Serialization;

namespace CoreBookingPlatform.AdapterService.Adapters.CdeAdapter
{
    public class CdeProductDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
        [JsonPropertyName("price")]
        public decimal Price { get; set; }
        [JsonPropertyName("currency")]
        public string Currency { get; set; } = "USD";
        [JsonPropertyName("sku")]
        public string SKU { get; set; } = string.Empty;
        [JsonPropertyName("categories")]
        public Dictionary<string, string> Categories { get; set; } = new Dictionary<string, string>();
        [JsonPropertyName("attributes")]
        public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();
    }
}
