using AutoMapper;
using CoreBookingPlatform.ProductService.Data.Context;
using CoreBookingPlatform.ProductService.Models.DTOs;
using CoreBookingPlatform.ProductService.Models.Entities;
using CoreBookingPlatform.ProductService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoreBookingPlatform.ProductService.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly ProductDbContext _productDbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(ProductDbContext productDbContext, IMapper mapper, ILogger<CategoryService> logger)
        {
            _productDbContext = productDbContext;
            _mapper = mapper;
            _logger = logger;
        }
        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto)
        {
            using var transaction = await _productDbContext.Database.BeginTransactionAsync();
            try
            {
                if (await _productDbContext.Categories.AnyAsync(c => c.Name == createCategoryDto.Name))
                    throw new ArgumentException($"A category with the name '{createCategoryDto.Name}' already exists.");

                var category = _mapper.Map<Category>(createCategoryDto);
                category.CreatedAt = DateTime.UtcNow;
                category.UpdatedAt = DateTime.UtcNow;

                _productDbContext.Categories.Add(category);
                await _productDbContext.SaveChangesAsync();

                if (createCategoryDto.ProductId.HasValue)
                {
                    var product = await _productDbContext.Products
                        .FirstOrDefaultAsync(p => p.ProductId == createCategoryDto.ProductId.Value);

                    if (product == null)
                    {
                        throw new ArgumentException($"product with id {createCategoryDto.ProductId} not exist");
                    }

                    var productCategory = new ProductCategory
                    {
                        ProductId = createCategoryDto.ProductId.Value,
                        CategoryId = category.CategoryId,
                        CreatedAt = DateTime.UtcNow,
                    };
                    _productDbContext.ProductCategories.Add(productCategory);
                    await _productDbContext.SaveChangesAsync();
                }
                await transaction.CommitAsync();

                return _mapper.Map<CategoryDto>(category);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating category: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            try
            {
                var categories = await _productDbContext.Categories.ToListAsync();
                return _mapper.Map<IEnumerable<CategoryDto>>(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories");
                throw;
            }
        }

        public async Task<CategoryDto> GetCategoryByIdAsync(int id)
        {
            try
            {
                var category = await _productDbContext.Categories
                    .FirstOrDefaultAsync(c => c.CategoryId == id);
                if (category == null)
                {
                    _logger.LogWarning("Category with ID {Id} not found", id);
                    return null;
                }
                else
                {
                    return _mapper.Map<CategoryDto>(category);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving category with Id {Id}", id);
                throw;
            }
        }

        public async Task<bool> UpdateCategoryAsync(int id, UpdateCategoryDto updateCategoryDto)
        {
            using var transaction = await _productDbContext.Database.BeginTransactionAsync();
            try
            {
                var category = await _productDbContext.Categories
                    .Include(c => c.ProductCategories)
                    .FirstOrDefaultAsync(c => c.CategoryId == id);
                if (category == null) return false;

                if (await _productDbContext.Categories.AnyAsync(c => c.Name == updateCategoryDto.Name && c.CategoryId != id))
                    throw new ArgumentException($"A category with the name '{updateCategoryDto.Name}' already exists.");

                _mapper.Map(updateCategoryDto, category);
                category.UpdatedAt = DateTime.UtcNow;

                await _productDbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error updating category with ID {Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            using var transaction = await _productDbContext.Database.BeginTransactionAsync();
            try
            {
                var category = await _productDbContext.Categories
                    .Include(c => c.ProductCategories)
                    .FirstOrDefaultAsync(c => c.CategoryId == id);
                if (category == null) return false;
                if (category.ProductCategories.Any())
                {
                    _productDbContext.ProductCategories.RemoveRange(category.ProductCategories);
                }

                _productDbContext.Categories.Remove(category);
                await _productDbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error deleting category with ID {Id}", id);
                throw;
            }
        
        }

    }
}
