using CoreBookingPlatform.ProductService.Models.DTOs;
using CoreBookingPlatform.ProductService.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CoreBookingPlatform.ProductService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttributeController : ControllerBase
    {
        private readonly IAttributeService _attributeService;
        private readonly ILogger<AttributeController> _logger;

        public AttributeController(IAttributeService service, ILogger<AttributeController> logger)
        {
            _attributeService = service;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(typeof(ProductAttributeDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProductAttributeDto>> CreateAttribute(int productId, CreateProductAttributeDto createAttributeDto)
        {
            try
            {
                var attribute = await _attributeService.CreateAttributeAsync(productId, createAttributeDto);
                return CreatedAtAction(nameof(GetAttributeById), new { productId, id = attribute.ProductAttributeId }, attribute);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request to create attribute for product ID {ProductId}", productId);
                return NotFound(ex.Message); // Use NotFound for invalid productId
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating attribute for product ID {ProductId}", productId);
                return BadRequest("An error occurred while creating the attribute.");
            }
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ProductAttributeDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProductAttributeDto>>> GetAllAttributes()
        {
            var attributes = await _attributeService.GetAllAttributesAsync();
            return Ok(attributes);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ProductAttributeDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductAttributeDto>> GetAttributeById(int id)
        {
            var attribute = await _attributeService.GetAttributeByIdAsync(id);
            if (attribute == null) return NotFound();
            return Ok(attribute);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAttribute(int id, UpdateProductAttributeDto updateAttributeDto)
        {
            try
            {
                var result = await _attributeService.UpdateAttributeAsync(id, updateAttributeDto);
                if (!result) return NotFound();
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAttribute(int id)
        {
            try
            {
                var result = await _attributeService.DeleteAttributeAsync(id);
                if (!result) return NotFound();
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
