using System.Text.Json.Serialization;

namespace CoreBookingPlatform.AdapterService.Adapters.AbcAdapter
{
    public class AbcContentDto
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
