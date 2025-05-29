namespace CoreBookingPlatform.ProductService.Models.DTOs
{
    public class ProductContentDto
    {
        public int ProductContentId { get; set; }
        public int ProductId { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? MediaUrl { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
