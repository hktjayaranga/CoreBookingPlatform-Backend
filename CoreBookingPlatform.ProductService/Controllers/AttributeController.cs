using CoreBookingPlatform.ProductService.Models.DTOs;
using CoreBookingPlatform.ProductService.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CoreBookingPlatform.ProductService.Controllers
{
    [Route("api/attribute")]
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
                if (attribute == null)
                {
                    return NotFound();
                }
                return CreatedAtAction(nameof(GetAttributeById), new { productId, id = attribute.ProductAttributeId }, attribute);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating attribute for product ID {ProductId}", productId);
                return BadRequest();
            }
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ProductAttributeDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<ProductAttributeDto>>> GetAllAttributes()
        {
            try
            {
                var attributes = await _attributeService.GetAllAttributesAsync();
                if (attributes == null)
                {
                    return NotFound();
                }
                return Ok(attributes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all attributes.");
                return BadRequest();
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ProductAttributeDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProductAttributeDto>> GetAttributeById(int id)
        {
            try
            {
                var attribute = await _attributeService.GetAttributeByIdAsync(id);
                if (attribute == null)
                {
                    return NotFound();
                }
                return Ok(attribute);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching attribute with ID {id}.");
                return BadRequest();
            }
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
                if (!result)
                {
                    return NotFound();
                }
                else
                {
                    return NoContent();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating attribute with ID {id}.");
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteAttribute(int id)
        {
            try
            {
                var result = await _attributeService.DeleteAttributeAsync(id);
                if (!result)
                {
                    return NotFound();
                }
                else
                {
                    return NoContent();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting attribute with ID {id}.");
                return BadRequest(ex.Message);
            }
        }
    }
}