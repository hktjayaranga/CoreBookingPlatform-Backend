using CoreBookingPlatform.ProductService.Models.DTOs;
using CoreBookingPlatform.ProductService.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CoreBookingPlatform.ProductService.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IProductService service, ILogger<ProductController> logger)
        {
            _productService = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetAllProducts()
        {
            try
            {
                var products = await _productService.GetAllProductsAsync();
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching all products.");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProductById(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                {
                    return NotFound();
                }
                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while fetching product with ID {id}.");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("external")]
        public async Task<ActionResult<ProductDto>> GetByExternal([FromQuery] string externalId, [FromQuery] string externalSystemName)
        {
            try
            {
                var all = await _productService.GetAllProductsAsync();
                var match = all.FirstOrDefault(p =>
                    p.ExternalId == externalId &&
                    p.ExternalSystemName == externalSystemName);

                if (match == null)
                {
                    return NotFound();
                }
                else
                {
                    return Ok(match);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching product by external identifiers.");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductDto createProductDto)
        {
            try
            {
                var createProduct = await _productService.CreateProductAsync(createProductDto);
                if(createProduct == null)
                {
                    return Conflict("Product already exists");
                }
                return CreatedAtAction(nameof(GetProductById), new { id = createProduct.ProductId }, createProduct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating a product.");
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateProduct(int id,UpdateProductDto updateProductDto)
        {
            try
            {
                var result = await _productService.UpdateProductAsync(id, updateProductDto);
                if(!result)
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
                _logger.LogError(ex, $"Error while updating product with ID {id}.");
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateProductCore(int id, [FromBody] UpdateProductCoreDto dto)
        {
            try
            {
                var updated = await _productService.UpdateProductCoreAsync(id, dto);
                if (updated)
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
                _logger.LogError(ex, $"Error while updating product core with ID {id}.");
                return BadRequest(ex.Message);
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var result = await _productService.DeleteProductAsync(id);
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
                _logger.LogError(ex, $"Error while deleting product with ID {id}.");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("content")]
        public async Task<ActionResult<ProductContentDto>> CreateProductContent(CreateProductContentDto createContentDto)
        {
            try
            {
                var content = await _productService.CreateProductContentAsync(createContentDto);
                return CreatedAtAction(nameof(GetProductContentById), new { id = content.ProductContentId }, content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating product content.");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("content/{id}")]
        public async Task<ActionResult<ProductContentDto>> GetProductContentById(int id)
        {
            try
            {
                var content = await _productService.GetProductContentByIdAsync(id);
                if (content == null)
                {
                    return NotFound();
                }
                return Ok(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while fetching product content with ID {id}.");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("content")]
        public async Task<ActionResult<ProductContentDto>> GetByProductContentTypeAndTitle([FromQuery] int productId, [FromQuery] string contentType, [FromQuery] string title)
        {
            try
            {
                var existing = await _productService.FindProductContentAsync(productId, contentType, title);
                if (existing == null)
                {
                    return NotFound();
                }
                return Ok(existing);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching product content by type and title.");
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("content/{id}")]
        public async Task<IActionResult> UpdateProductContent(int id, UpdateProductContentDto updateContentDto)
        {
            try
            {
                var result = await _productService.UpdateProductContentAsync(id, updateContentDto);
                if(!result)
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
                _logger.LogError(ex, $"Error while updating product content with ID {id}.");
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("content/{id}")]
        public async Task<IActionResult> DeleteProductContent(int id)
        {
            try
            {
                var result = await _productService.DeleteProductContentAsync(id);
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
                _logger.LogError(ex, $"Error while deleting product content with ID {id}.");
                return BadRequest(ex.Message);
            }
        }
    }
}
