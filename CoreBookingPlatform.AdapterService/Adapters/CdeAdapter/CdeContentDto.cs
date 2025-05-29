using System.Text.Json.Serialization;

namespace CoreBookingPlatform.AdapterService.Adapters.CdeAdapter
{
    public class CdeContentDto
    {
        [JsonPropertyName("productId")]
        public string ProductId { get; set; } = string.Empty;
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;
        [JsonPropertyName("description")]
        public string? Description { get; set; }
        [JsonPropertyName("mediaUrl")]
        public string? MediaUrl { get; set; }
        [JsonPropertyName("order")]
        public int Order { get; set; }
    }
}
