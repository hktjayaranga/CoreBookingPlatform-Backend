namespace CoreBookingPlatform.ProductService.Models.DTOs
{
    public class ProductAttributeDto
    {
        public int ProductAttributeId { get; set; }
        public string Name { get; set; } = string.Empty;
        //public string? Description { get; set; }
        public string Value {  get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
