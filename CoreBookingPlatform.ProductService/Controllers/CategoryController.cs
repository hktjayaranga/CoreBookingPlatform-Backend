using CoreBookingPlatform.ProductService.Models.DTOs;
using CoreBookingPlatform.ProductService.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CoreBookingPlatform.ProductService.Controllers
{
    [Route("api/category")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController (ICategoryService categoryService, ILogger<CategoryController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<CategoryDto>> CreateCategory(CreateCategoryDto createCategoryDto)
        {
            try
            {
                var category = await _categoryService.CreateCategoryAsync(createCategoryDto);
                //return CreatedAtAction(nameof(GetCategoryById), new { id = category.CategoryId }, category);
                if (category == null)
                {
                    return NotFound();
                }
                return Ok(category);
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating a new category.");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAllCategories()
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                if(categories == null)
                {
                    return NotFound();
                }
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching all categories.");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDto>> GetCategoryById(int id)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                if (category == null)
                {
                    return NotFound();
                }
                return Ok(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while fetching category with ID {id}.");
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, UpdateCategoryDto updateCategoryDto)
        {
            try
            {
                var result = await _categoryService.UpdateCategoryAsync(id, updateCategoryDto);
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
                _logger.LogError(ex, $"Error updating category with ID {id}.");
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var result = await _categoryService.DeleteCategoryAsync(id);
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
                return BadRequest(ex.Message);
            }
        }
    }
}
