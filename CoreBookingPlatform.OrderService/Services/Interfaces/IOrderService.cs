using CoreBookingPlatform.OrderService.Models.DTOs;

namespace CoreBookingPlatform.OrderService.Services.Interfaces
{
    public interface IOrderService
    {
        Task<OrderDto> CreateOrderAsync(string userId);
        Task<OrderDto> GetOrderByIdAsync(int id);
        Task<IEnumerable<OrderDto>> GetOrdersByUserAsync(string userId);
        Task<bool> CancelOrderAsync(int id);
    }
}
