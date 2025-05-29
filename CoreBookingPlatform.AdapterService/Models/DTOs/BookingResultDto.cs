namespace CoreBookingPlatform.AdapterService.Models.DTOs
{
    public class BookingResultDto
    {
        public string ExternalProductId { get; set; } = "";
        public string BookingId { get; set; } = "";
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
