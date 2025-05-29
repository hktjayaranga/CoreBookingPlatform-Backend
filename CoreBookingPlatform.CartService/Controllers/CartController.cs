using CoreBookingPlatform.CartService.Models.DTOs;
using CoreBookingPlatform.CartService.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;

namespace CoreBookingPlatform.CartService.Controllers
{
    [Route("api/cart")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly ILogger<CartController> _logger;

        public CartController(ICartService cartService, ILogger<CartController> logger)
        {
            _cartService = cartService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<CartDto>> GetCart([FromQuery] string userId)
        {
            try
            {
                var cart = await _cartService.GetCartAsync(userId);
                return Ok(cart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cart for UserId {UserId}", userId);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("items")]
        public async Task<ActionResult<CartItemDto>> AddItem([FromQuery] string userId, [FromBody] CreateCartItemDto createCartItemDto)
        {
            try
            {
                var item = await _cartService.AddItemAsync(userId, createCartItemDto);
                return CreatedAtAction(nameof(GetCart), new { userId }, item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to cart for UserId {UserId}", userId);
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("items/{itemId}")]
        public async Task<IActionResult> RemoveItem(int itemId, [FromQuery] string userId)
        {
            try
            {
                var result = await _cartService.RemoveItemAsync(userId, itemId);
                return result ? NoContent() : NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing item {ItemId} from cart for UserId {UserId}", itemId, userId);
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("items/{itemId}")]
        public async Task<IActionResult> UpdateItem(int itemId, [FromBody] UpdateCartItemDto updateCartItemDto, [FromQuery] string userId)
        {
            try
            {
                var result = await _cartService.UpdateItemAsync(userId, itemId, updateCartItemDto);
                return result ? NoContent() : NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating item {ItemId} in cart for UserId {UserId}", itemId, userId);
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        public async Task<IActionResult> ClearCart([FromQuery] string userId)
        {
            try
            {
                var result = await _cartService.ClearCartAsync(userId);
                return result ? NoContent() : NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart for UserId {UserId}", userId);
                return BadRequest(ex.Message);
            }
        }



    }
}
