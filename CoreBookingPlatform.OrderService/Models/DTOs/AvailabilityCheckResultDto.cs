namespace CoreBookingPlatform.OrderService.Models.DTOs
{
    public class AvailabilityCheckResultDto
    {
        public bool IsAvailable { get; set; }
        public List<UnavailableItemDto> UnavailableItems { get; set; } = new List<UnavailableItemDto>();
    }
}
