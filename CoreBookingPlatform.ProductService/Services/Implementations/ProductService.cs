using AutoMapper;
using CoreBookingPlatform.ProductService.Data.Context;
using CoreBookingPlatform.ProductService.Models.DTOs;
using CoreBookingPlatform.ProductService.Models.Entities;
using CoreBookingPlatform.ProductService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoreBookingPlatform.ProductService.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly ProductDbContext _productDbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductService> _logger;

        public ProductService(ProductDbContext productDbContext, IMapper mapper, ILogger<ProductService> logger)
        {
            _productDbContext = productDbContext;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
        {
            using var transaction = await _productDbContext.Database.BeginTransactionAsync();
            try
            {
                if (!string.IsNullOrWhiteSpace(dto.ExternalId) && !string.IsNullOrWhiteSpace(dto.ExternalSystemName))
                {
                    var existingProduct = await _productDbContext.Products
                        .Include(p => p.ProductCategories).ThenInclude(pc => pc.Category)
                        .Include(p => p.Attributes)
                        .Include(p => p.Contents)
                        .FirstOrDefaultAsync(p =>
                            p.ExternalId == dto.ExternalId &&
                            p.ExternalSystemName == dto.ExternalSystemName);

                    if (existingProduct != null)
                    {
                        _logger.LogInformation("Product already exists. Returning existing product with ID: {ProductId}", existingProduct.ProductId);
                        return null;
                    }
                }

                var product = _mapper.Map<Product>(dto);
                product.CreatedAt = DateTime.UtcNow;
                product.LastUpdatedAt = DateTime.UtcNow;

                var descriptionList = dto.Categories
                    .Select(c => (c.Name, c.Description))
                    .Distinct()
                    .ToList();

                var existingCategories = await _productDbContext.Categories
                    .Where(c => descriptionList.Select(nd => nd.Name).Contains(c.Name))
                    .ToDictionaryAsync(c => c.Name, c => c);

                var newCategories = descriptionList
                    .Where(d => !existingCategories.ContainsKey(d.Name))
                    .Select(d => new Category
                    {
                        Name = d.Name,
                        Description = d.Description,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }).ToList();

                _productDbContext.Categories.AddRange(newCategories);
                await _productDbContext.SaveChangesAsync();

                foreach (var category in newCategories)
                {
                    existingCategories[category.Name] = category;
                }

                foreach (var (name, desc) in descriptionList)
                {
                    product.ProductCategories.Add(new ProductCategory
                    {
                        CategoryId = existingCategories[name].CategoryId,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                foreach (var attributeDto in dto.Attributes)
                {
                    product.Attributes.Add(new ProductAttribute
                    {
                        Name = attributeDto.Name,
                        Value = attributeDto.Value,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }

                foreach (var contentDto in dto.Contents ?? Enumerable.Empty<CreateProductContentDto>())
                {
                    product.Contents.Add(new ProductContent
                    {
                        ContentType = contentDto.ContentType,
                        Title = contentDto.Title,
                        Description = contentDto.Description,
                        MediaUrl = contentDto.MediaUrl,
                        SortOrder = contentDto.SortOrder,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }

                _productDbContext.Products.Add(product);
                await _productDbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                var createdProduct = await _productDbContext.Products
                    .Include(p => p.ProductCategories).ThenInclude(pc => pc.Category)
                    .Include(p => p.Attributes)
                    .Include(p => p.Contents)
                    .FirstOrDefaultAsync(p => p.ProductId == product.ProductId);

                _logger.LogInformation("Product and related data created successfully with ID: {ProductId}", product.ProductId);
                return _mapper.Map<ProductDto>(createdProduct);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to create product with all related data: {Message}", ex.Message);
                throw;
            }
        }

        private async Task<List<int>> GetOrCreateCategoryIdsAsync(List<(string Name, string? Description)> categories)
        {
            var names = categories.Select(c => c.Name).Distinct().ToList();
            var existing = await _productDbContext.Categories
                .Where(c => names.Contains(c.Name))
                .ToDictionaryAsync(c => c.Name, c => c);

            var ids = new List<int>();
            foreach (var (name, desc) in categories)
            {
                if (existing.TryGetValue(name, out var category))
                {
                    ids.Add(category.CategoryId);
                }
                else
                {
                    var newCategory = new Category
                    {
                        Name = name,
                        Description = desc,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _productDbContext.Categories.Add(newCategory);
                    await _productDbContext.SaveChangesAsync();
                    ids.Add(newCategory.CategoryId);
                    existing[name] = newCategory;
                }
            }
            return ids;
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            try
            {
                var products = await _productDbContext.Products
                    .Include(p => p.ProductCategories).ThenInclude(pc => pc.Category)
                    .Include(p => p.Attributes)
                    .Include(p => p.Contents.OrderBy(c => c.SortOrder))
                    .ToListAsync();

                return _mapper.Map<IEnumerable<ProductDto>>(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products");
                throw;
            }
        }

        public async Task<ProductDto> GetProductByIdAsync(int id)
        {
            try
            {
                var product = await _productDbContext.Products
                    .Include(p => p.ProductCategories).ThenInclude(pc => pc.Category)
                    .Include(p => p.Attributes)
                    .Include(p => p.Contents)
                    .FirstOrDefaultAsync(p => p.ProductId == id);

                if (product == null)
                {
                    _logger.LogWarning("Product with ID {Id} not found", id);
                    return null;
                }
                else
                {
                    return _mapper.Map<ProductDto>(product);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product with ID {Id}", id);
                throw;
            }
        }

        public async Task<bool> UpdateProductAsync(int id, UpdateProductDto updateProductDto)
        {
            using var transaction = await _productDbContext.Database.BeginTransactionAsync();
            try
            {
                var product = await _productDbContext.Products
                    .Include(p => p.ProductCategories)
                    .Include(p => p.Attributes)
                    .Include(p => p.Contents)
                    .FirstOrDefaultAsync(p => p.ProductId == id);

                if (product == null) return false;

                _mapper.Map(updateProductDto, product);
                product.LastUpdatedAt = DateTime.UtcNow;

                var categoryInput = updateProductDto.Categories
                    .Select(c => (Name: c.Name, Description: c.Description)).ToList();

                var newCategoryIds = await GetOrCreateCategoryIdsAsync(categoryInput);
                var existingCategoryIds = product.ProductCategories.Select(pc => pc.CategoryId).ToList();

                var categoriesToRemove = existingCategoryIds.Except(newCategoryIds).ToList();
                var categoriesToAdd = newCategoryIds.Except(existingCategoryIds).ToList();

                foreach (var categoryId in categoriesToRemove)
                {
                    var pc = product.ProductCategories.First(pc => pc.CategoryId == categoryId);
                    product.ProductCategories.Remove(pc);
                }

                foreach (var categoryId in categoriesToAdd)
                {
                    product.ProductCategories.Add(new ProductCategory
                    {
                        CategoryId = categoryId,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                foreach (var attributeDto in updateProductDto.Attributes)
                {
                    var existingAttr = product.Attributes.FirstOrDefault(a => a.Name == attributeDto.Name);
                    if (existingAttr != null)
                    {
                        existingAttr.Value = attributeDto.Value;
                        existingAttr.UpdatedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        product.Attributes.Add(new ProductAttribute
                        {
                            Name = attributeDto.Name,
                            Value = attributeDto.Value,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        });
                    }
                }

                foreach (var contentDto in updateProductDto.Contents)
                {
                    var existingContent = product.Contents.FirstOrDefault(c => c.ContentType == contentDto.ContentType && c.Title == contentDto.Title);
                    if (existingContent != null)
                    {
                        existingContent.ContentType = contentDto.ContentType;
                        existingContent.Title = contentDto.Title;
                        existingContent.Description = contentDto.Description;
                        existingContent.MediaUrl = contentDto.MediaUrl;
                        existingContent.SortOrder = contentDto.SortOrder;
                        existingContent.UpdatedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        product.Contents.Add(new ProductContent
                        {
                            ProductId = product.ProductId,
                            ContentType = contentDto.ContentType,
                            Title = contentDto.Title,
                            Description = contentDto.Description,
                            MediaUrl = contentDto.MediaUrl,
                            SortOrder = contentDto.SortOrder,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        });
                    }
                }

                await _productDbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product with ID {Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            using var transaction = await _productDbContext.Database.BeginTransactionAsync();
            try
            {
                var product = await _productDbContext.Products
                    .Include(p => p.Attributes)
                    .Include(p => p.Contents)
                    .Include(p => p.ProductCategories)
                    .FirstOrDefaultAsync(p => p.ProductId == id);

                if (product == null) return false;

                var categoryIds = product.ProductCategories
                    .Select(c => c.CategoryId).ToList();

                _productDbContext.Products.Remove(product);
                await _productDbContext.SaveChangesAsync();

                var orphanedCategories = await _productDbContext.Categories
                    .Where(c => categoryIds.Contains(c.CategoryId))
                    .Include(c => c.ProductCategories)
                    .Where(c => !c.ProductCategories.Any())
                    .ToListAsync();

                if (orphanedCategories.Any())
                {
                    _productDbContext.Categories.RemoveRange(orphanedCategories);
                    await _productDbContext.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product with ID {Id}", id);
                throw;
            }
        }

        public async Task<ProductContentDto> CreateProductContentAsync(CreateProductContentDto createContentDto)
        {
            try
            {
                var content = _mapper.Map<ProductContent>(createContentDto);
                content.CreatedAt = DateTime.UtcNow;
                content.UpdatedAt = DateTime.UtcNow;

                _productDbContext.ProductContent.Add(content);
                await _productDbContext.SaveChangesAsync();

                return _mapper.Map<ProductContentDto>(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product content");
                throw;
            }
        }

        public async Task<ProductContentDto> GetProductContentByIdAsync(int id)
        {
            try
            {
                var content = await _productDbContext.ProductContent.FindAsync(id);
                if (content == null)
                {
                    return null;
                }
                else
                {
                    return _mapper.Map<ProductContentDto>(content);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product content with ID {Id}", id);
                throw;
            }
        }

        public async Task<ProductContentDto> FindProductContentAsync(int productId, string contentType, string title)
        {
            try
            {
                var entity = await _productDbContext.ProductContent
                    .Where(c => c.ProductId == productId
                             && c.ContentType == contentType
                             && c.Title == title)
                    .FirstOrDefaultAsync();

                if (entity == null)
                {
                    return null;
                }
                else
                {
                    return _mapper.Map<ProductContentDto>(entity);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FindProductContentAsync(productId={ProductId}, contentType={ContentType}, title={Title}).",
                                  productId, contentType, title);
                throw;
            }
        }

        public async Task<bool> UpdateProductContentAsync(int id, UpdateProductContentDto updateContentDto)
        {
            try
            {
                var content = await _productDbContext.ProductContent.FindAsync(id);
                if (content == null) return false;

                _mapper.Map(updateContentDto, content);
                content.UpdatedAt = DateTime.UtcNow;

                await _productDbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product content with ID {Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteProductContentAsync(int id)
        {
            try
            {
                var content = await _productDbContext.ProductContent.FindAsync(id);
                if (content == null) return false;

                _productDbContext.ProductContent.Remove(content);
                await _productDbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product content with ID {Id}", id);
                throw;
            }
        }

        public async Task<bool> UpdateProductCoreAsync(int id, UpdateProductCoreDto dto)
        {
            try
            {
                var product = await _productDbContext.Products.FindAsync(id);
                if (product == null) return false;

                product.ProductName = dto.ProductName;
                product.ProductDescription = dto.ProductDescription;
                product.BasePrice = dto.BasePrice;
                product.Currency = dto.Currency;
                product.SKU = dto.SKU;
                product.LastUpdatedAt = DateTime.UtcNow;

                await _productDbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating core product details with ID {Id}", id);
                throw;
            }
        }
    }
}