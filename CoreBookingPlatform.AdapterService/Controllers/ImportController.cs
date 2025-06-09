using CoreBookingPlatform.AdapterService.Interfaces;
using CoreBookingPlatform.AdapterService.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CoreBookingPlatform.AdapterService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImportController : ControllerBase
    {
        private readonly IEnumerable<IAdapter> _adapters;
        private readonly ILogger<ImportController> _logger;

        public ImportController(IEnumerable<IAdapter> adapters, ILogger<ImportController> logger)
        {
            _adapters = adapters;
            _logger = logger;
        }

        [HttpPost("products-and-content")]
        public async Task<IActionResult> ImportProductsAndContent()
        {
            try
            {
                
                    foreach(var adapter in _adapters)
                    {
                        await adapter.ImportProductsAndContentAsync();
                    }
                    return Ok("Product and content import triggered for all exteranl systems");
                
                
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error importing products and content.");
                return StatusCode(500, "Error importing products and content.");
            }
        }

        [HttpGet("availability")]
        public async Task<IActionResult> CheckAvailability([FromQuery] string externalId, [FromQuery] string externalSystemName)
        {
            try
            {
                var adapter = _adapters.FirstOrDefault(a => a.ExternalSystemName == externalSystemName);
                if (adapter == null)
                {
                    _logger.LogWarning("Adapter for {ExternalSystemName} not found.", externalSystemName);
                    return BadRequest("Invalid external system name.");
                }

                var availability = await adapter.CheckAvailabilityAsync(externalId);
                return Ok(availability);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking availability for product {ExternalId} in system {ExternalSystemName}", externalId, externalSystemName);
                return StatusCode(500, "Error checking availability.");
            }
        }

        [HttpPost("bookings")]
        public async Task<IActionResult> CreateBookings([FromQuery] string externalSystemName, [FromBody] List<BookingItemDto> items)
        {
            var adapter = _adapters.FirstOrDefault(a => a.ExternalSystemName == externalSystemName);
            if (adapter == null)
            {
                _logger.LogWarning("Adapter for {ExternalSystemName} not found.", externalSystemName);
                return BadRequest("Invalid external system name.");
            }

            var results = await adapter.CreateBookingAsync(items);
            return Ok(results);
        }

        [HttpPost("bookings/cancel")]
        public async Task<IActionResult> CancelBookings([FromQuery] string externalSystemName, [FromBody] List<string> bookingIds)
        {
            var adapter = _adapters.FirstOrDefault(a => a.ExternalSystemName == externalSystemName);
            if (adapter == null)
            {
                _logger.LogWarning("Adapter for {ExternalSystemName} not found.", externalSystemName);
                return BadRequest("Invalid external system name.");
            }

            var results = await adapter.CancelBookingsAsync(bookingIds);
            return Ok(results);
        }
    }
}