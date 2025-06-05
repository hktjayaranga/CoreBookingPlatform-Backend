using CoreBookingPlatform.OrderService.Models.DTOs;
using CoreBookingPlatform.OrderService.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CoreBookingPlatform.OrderService.Controllers
{
    [Route("api/orders")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IOrderService orderService, ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<OrderDto>> CreateOrder([FromQuery] string userId)
        {
            try
            {
                var order = await _orderService.CreateOrderAsync(userId);
                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating an order for user {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error while creating the order.");
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetAllOrders()
        {
            try
            {
                var orders = await _orderService.GetAllOrdersAsync();
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while retrieving all orders.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error while retrieving orders.");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> GetOrderById(int id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);
                if (order != null)
                {
                    return Ok(order);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while retrieving order with ID {OrderId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error while retrieving the order.");
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrdersByUser(string userId)
        {
            try
            {
                var orders = await _orderService.GetOrdersByUserAsync(userId);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while retrieving orders for user {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error while retrieving the user's orders.");
            }
        }

        [HttpPost("check-availability")]
        public async Task<ActionResult<AvailabilityCheckResultDto>> CheckAvailability([FromQuery] string userId)
        {
            try
            {
                var result = await _orderService.CheckCartAvailabilityAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while checking cart availability for user {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error while checking cart availability.");
            }
        }

        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            try
            {
                var result = await _orderService.CancelOrderAsync(id);
                if (result)
                {
                    return NoContent();
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while canceling order with ID {OrderId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error while canceling the order.");
            }
        }
    }
}