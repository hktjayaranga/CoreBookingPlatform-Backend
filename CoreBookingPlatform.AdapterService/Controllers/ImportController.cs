using CoreBookingPlatform.AdapterService.Adapters.AbcAdapter;
using CoreBookingPlatform.AdapterService.Interfaces;
using CoreBookingPlatform.AdapterService.Models.DTOs;
using Microsoft.AspNetCore.Http;
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
        [HttpPost("products")]
        public async Task<IActionResult> ImportProducts([FromQuery] string externalSystemName)
        {
            var adapter = _adapters.FirstOrDefault(a => a.ExternalSystemName == externalSystemName);
            if (adapter == null)
            {
                _logger.LogWarning("Adapter for {ExternalSystemName} not found.", externalSystemName);
                return BadRequest("Invalid external system name.");
            }

            await adapter.ImportProductsAsync();
            return Ok($"Product import triggered for {externalSystemName}.");
        }

        [HttpPost("content/{externalProductId}")]
        public async Task<IActionResult> ImportProductContent(string externalProductId, [FromQuery] string externalSystemName)
        {
            var adapter = _adapters.FirstOrDefault(a => a.ExternalSystemName == externalSystemName);
            if (adapter == null)
            {
                _logger.LogWarning("Adapter for {ExternalSystemName} not found.", externalSystemName);
                return BadRequest("Invalid external system name.");
            }

            await adapter.ImportProductContentAsync(externalProductId);
            return Ok($"Content import triggered for product {externalProductId} for {externalSystemName}.");
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
    }
}
