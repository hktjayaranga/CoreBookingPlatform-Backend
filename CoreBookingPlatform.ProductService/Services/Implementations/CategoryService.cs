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

        //public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto)
        //{
        //    try
        //    {
        //        var category = _mapper.Map<Category>(createCategoryDto);
        //        category.CreatedAt = DateTime.UtcNow;
        //        category.UpdatedAt = DateTime.UtcNow;
        //        _productDbContext.Categories.Add(category);
        //        await _productDbContext.SaveChangesAsync();
        //        return _mapper.Map<CategoryDto>(category);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error creating category");
        //        throw;
        //    }
        //}

        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto)
        {
            using var transaction = await _productDbContext.Database.BeginTransactionAsync();
            try
            {
                // Validate ParentCategoryId
                //if (createCategoryDto.ParentCategoryId.HasValue)
                //{
                //    var parentExists = await _productDbContext.Categories.AnyAsync(c => c.CategoryId == createCategoryDto.ParentCategoryId);
                //    if (!parentExists)
                //        throw new ArgumentException($"Parent category with ID {createCategoryDto.ParentCategoryId} does not exist.");
                //}

                if (await _productDbContext.Categories.AnyAsync(c => c.Name == createCategoryDto.Name))
                    throw new ArgumentException($"A category with the name '{createCategoryDto.Name}' already exists.");

                var category = _mapper.Map<Category>(createCategoryDto);
                category.CreatedAt = DateTime.UtcNow;
                category.UpdatedAt = DateTime.UtcNow;

                _productDbContext.Categories.Add(category);
                await _productDbContext.SaveChangesAsync();
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
                //if (category == null) return null;
                //return _mapper.Map<CategoryDto>(category);
                return category == null ? null : _mapper.Map<CategoryDto>(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving category with Id {Id}", id);
                throw;
            }
        }

        //public async Task<bool> UpdateCategoryAsync(int id, UpdateCategoryDto updateCategoryDto)
        //{
        //    try
        //    {
        //        var category = await _productDbContext.Categories.FirstOrDefaultAsync(c => c.CategoryId == id);
        //        if (category == null) return false;

        //        _mapper.Map(updateCategoryDto, category);
        //        category.UpdatedAt = DateTime.UtcNow;
        //        await _productDbContext.SaveChangesAsync();
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error updating category with ID {Id}", id);
        //        throw;
        //    }
        //}
        public async Task<bool> UpdateCategoryAsync(int id, UpdateCategoryDto updateCategoryDto)
        {
            using var transaction = await _productDbContext.Database.BeginTransactionAsync();
            try
            {
                var category = await _productDbContext.Categories
                    .Include(c => c.ProductCategories)
                    .FirstOrDefaultAsync(c => c.CategoryId == id);
                if (category == null) return false;

                // Validate ParentCategoryId
                //if (updateCategoryDto.ParentCategoryId.HasValue)
                //{
                //    var parentExists = await _productDbContext.Categories.AnyAsync(c => c.CategoryId == updateCategoryDto.ParentCategoryId);
                //    if (!parentExists)
                //        throw new ArgumentException($"Parent category with ID {updateCategoryDto.ParentCategoryId} does not exist.");
                //    // Prevent self-referencing
                //    if (updateCategoryDto.ParentCategoryId == id)
                //        throw new ArgumentException("A category cannot be its own parent.");
                //}

                // Check for duplicate name (excluding current category)
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

        //public async Task<bool> DeleteCategoryAsync(int id)
        //{
        //    try
        //    {
        //        var category = await _productDbContext.Categories.FirstOrDefaultAsync(c => c.CategoryId == id);
        //        if (category == null) return false;

        //        _productDbContext.Categories.Remove(category);
        //        await _productDbContext.SaveChangesAsync();
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error deleting category with ID {Id}", id);
        //        throw;
        //    }
        //}
        public async Task<bool> DeleteCategoryAsync(int id)
        {
            using var transaction = await _productDbContext.Database.BeginTransactionAsync();
            try
            {
                var category = await _productDbContext.Categories
                    //.Include(c => c.ChildCategories)
                    .Include(c => c.ProductCategories)
                    .FirstOrDefaultAsync(c => c.CategoryId == id);
                if (category == null) return false;

                // Check for child categories
                //if (category.ChildCategories.Any())
                //    throw new InvalidOperationException("Cannot delete a category that has child categories. Delete or reassign child categories first.");

                // Remove associated ProductCategories
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
