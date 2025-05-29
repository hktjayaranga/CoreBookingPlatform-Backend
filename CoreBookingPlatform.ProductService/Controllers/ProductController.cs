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
            var products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProductById(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        [HttpGet("external")]
        public async Task<ActionResult<ProductDto>> GetByExternal([FromQuery] string externalId,[FromQuery] string externalSystemName)
        {
            var all = await _productService.GetAllProductsAsync();
            var match = all.FirstOrDefault(p =>
                p.ExternalId == externalId &&
                p.ExternalSystemName == externalSystemName);

            return match is null ? NotFound() : Ok(match);
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
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateProduct(int id,UpdateProductDto updateProductDto)
        {
            try
            {
                var result = await _productService.UpdateProductAsync(id, updateProductDto);
                if (!result) return NotFound();
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchProductCore(int id, [FromBody] UpdateProductCoreDto dto)
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


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var result = await _productService.DeleteProductAsync(id);
                if (!result) return NotFound();
                return NoContent();

            }
            catch (Exception ex)
            {
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
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("content/{id}")]
        public async Task<ActionResult<ProductContentDto>> GetProductContentById(int id)
        {
            var content = await _productService.GetProductContentByIdAsync(id);
            if (content == null) return NotFound();
            return Ok(content);
        }

        [HttpPut("content/{id}")]
        public async Task<IActionResult> UpdateProductContent(int id, UpdateProductContentDto updateContentDto)
        {
            try
            {
                var result = await _productService.UpdateProductContentAsync(id, updateContentDto);
                if (!result) return NotFound();
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("content/{id}")]
        public async Task<IActionResult> DeleteProductContent(int id)
        {
            try
            {
                var result = await _productService.DeleteProductContentAsync(id);
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
