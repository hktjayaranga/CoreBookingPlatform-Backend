using CoreBookingPlatform.AdapterService.Models.DTOs;
using CoreBookingPlatform.ProductService.Models.DTOs;

namespace CoreBookingPlatform.AdapterService.Interfaces
{
    public interface IAdapter
    {
        string ExternalSystemName { get; }
        Task ImportProductsAndContentAsync();
        Task<ProductAvailabilityDto> CheckAvailabilityAsync(string externalProductId);
        Task<List<BookingResultDto>> CreateBookingAsync(List<BookingItemDto> items);
        Task<List<BookingResultDto>> CancelBookingsAsync(List<string> bookingIds);
    }
}
