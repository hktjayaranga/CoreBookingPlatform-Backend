using AutoMapper;
using CoreBookingPlatform.CartService.Data.Context;
using CoreBookingPlatform.CartService.Models.DTOs;
using CoreBookingPlatform.CartService.Models.Entities;
using CoreBookingPlatform.CartService.Services.Interfaces;
using CoreBookingPlatform.ProductService.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CoreBookingPlatform.CartService.Services.Implementations
{
    public class CartService : ICartService
    {
        private readonly CartDbContext _cartDbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<CartService> _logger;
        private readonly HttpClient _productServiceClient;

        public CartService(CartDbContext cartDbContext,IMapper mapper,ILogger<CartService> logger,IHttpClientFactory httpClientFactory)
        {
            _cartDbContext = cartDbContext;
            _mapper = mapper;
            _logger = logger;
            _productServiceClient = httpClientFactory.CreateClient("ProductService");

        }

        public async Task<CartDto> GetCartAsync(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                    throw new ArgumentException("UserId cannot be null or empty.");
                var user = await _cartDbContext.Users.FindAsync(userId);
                if (user == null)
                {
                    throw new Exception($"User with ID {userId} not found.");
                }
                var cart = await _cartDbContext.Carts
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.UserId == userId)
                    ?? new Cart{
                        UserId = userId,
                        Items = new List<CartItem>() 
                    };
                if (cart.CartId == 0)
                {
                    _cartDbContext.Carts.Add(cart);
                    await _cartDbContext.SaveChangesAsync();
                }
                var cartDto = _mapper.Map<CartDto>(cart);
                cartDto.TotalPrice = cart.Items.Sum(i => i.Price * i.Quantity);
                return cartDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cart for UserId {UserId}", userId);
                throw;
            }


        }

        public async Task<CartItemDto> AddItemAsync(string userId, CreateCartItemDto createCartItemDto)
        {
            try
            {
                var user = await _cartDbContext.Users.FindAsync(userId);
                if (user == null)
                {
                    throw new Exception($"User with ID {userId} not found.");
                }
                //get product details form product service
                var productResponse = await _productServiceClient.GetAsync($"api/products/{createCartItemDto.ProductId}");
                if (!productResponse.IsSuccessStatusCode)
                    throw new Exception("Product not found in ProductService.");
                var productJson = await productResponse.Content.ReadAsStringAsync();
                var product = JsonSerializer.Deserialize<ProductDto>(productJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (product == null)
                    throw new Exception("Failed to deserialize product.");


                var cart = await _cartDbContext.Carts
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.UserId == userId) ?? new Cart { UserId = userId };
                if (cart.CartId == 0)
                {
                    _cartDbContext.Carts.Add(cart);
                    var rows =  await _cartDbContext.SaveChangesAsync();
                    _logger.LogInformation("Saved changes, affected rows: {Rows}", rows);
                }

                var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == createCartItemDto.ProductId);
                if (existingItem != null)
                {
                    existingItem.Quantity += createCartItemDto.Quantity;
                    existingItem.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    var cartItem = new CartItem
                    {
                        CartId = cart.CartId,
                        ProductId = createCartItemDto.ProductId,
                        Quantity = createCartItemDto.Quantity,
                        Price = product.BasePrice,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    cart.Items.Add(cartItem);
                }
                cart.UpdatedAt = DateTime.UtcNow;
                await _cartDbContext.SaveChangesAsync();

                var cartItemDto = existingItem != null
                    ? _mapper.Map<CartItemDto>(existingItem)
                    : _mapper.Map<CartItemDto>(cart.Items.Last());
                return cartItemDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to cart for UserId {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> UpdateItemAsync(string userId, int itemId, UpdateCartItemDto updateCartItemDto)
        {
            try
            {
                var user = await _cartDbContext.Users.FindAsync(userId);
                if (user == null)
                    throw new Exception($"User with ID {userId} not found.");

                var cart = await _cartDbContext.Carts
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.UserId == userId);
                if (cart == null) return false;

                var item = cart.Items.FirstOrDefault(i => i.CartItemId == itemId);
                if (item == null) return false;

                item.Quantity = updateCartItemDto.Quantity;
                item.UpdatedAt = DateTime.UtcNow;
                cart.UpdatedAt = DateTime.UtcNow;
                await _cartDbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating item {ItemId} in cart for UserId {UserId}", itemId, userId);
                throw;
            }
        }

        public async Task<bool> RemoveItemAsync(string userId, int itemId)
        {
            try
            {
                var user = await _cartDbContext.Users.FindAsync(userId);
                if(user == null)
                {
                    throw new Exception($"User with ID {userId} not found.");
                }

                var cart = await _cartDbContext.Carts
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.UserId == userId);
                if (cart == null) return false;

                var item = cart.Items.FirstOrDefault(i => i.CartItemId == itemId);
                if (item == null) return false;

                cart.Items.Remove(item);
                cart.UpdatedAt = DateTime.UtcNow;
                await _cartDbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing item {ItemId} from cart for UserId {UserId}", itemId, userId);
                throw;
            }
        }

        public async Task<bool> ClearCartAsync(string userId)
        {
            try
            {
                var user = await _cartDbContext.Users.FindAsync(userId);
                if (user == null)
                    throw new Exception($"User with ID {userId} not found.");
                var cart = await _cartDbContext.Carts
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.UserId == userId);
                if (cart == null) return false;

                cart.Items.Clear();
                cart.UpdatedAt = DateTime.UtcNow;
                await _cartDbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart for UserId {UserId}", userId);
                throw;
            }
        }

        

        

        
    }
}
