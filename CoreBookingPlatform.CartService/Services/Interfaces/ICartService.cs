using CoreBookingPlatform.CartService.Models.DTOs;

namespace CoreBookingPlatform.CartService.Services.Interfaces
{
    public interface ICartService
    {
        Task<CartDto> GetCartAsync(string userId);
        Task<CartItemDto> AddItemAsync(string userId, CreateCartItemDto createCartItemDto);
        Task<bool> RemoveItemAsync(string userId, int itemId);
        Task<bool> UpdateItemAsync(string userId, int itemId, UpdateCartItemDto updateCartItemDto);
        Task<bool> ClearCartAsync(string userId);
    }
}
