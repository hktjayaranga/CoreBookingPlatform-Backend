using AutoMapper;
using CoreBookingPlatform.ProductService.Data.Context;
using CoreBookingPlatform.ProductService.Models.DTOs;
using CoreBookingPlatform.ProductService.Models.Entities;
using CoreBookingPlatform.ProductService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoreBookingPlatform.ProductService.Services.Implementations
{
    public class AttributeService : IAttributeService
    {
        private readonly ProductDbContext _productDbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<AttributeService> _logger;

        public AttributeService(ProductDbContext productDbContext, IMapper mapper, ILogger<AttributeService> logger)
        {
            _productDbContext = productDbContext;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ProductAttributeDto> CreateAttributeAsync(int productId, CreateProductAttributeDto createAttributeDto)
        {
            using var transaction = await _productDbContext.Database.BeginTransactionAsync();
            try
            {
                var product = await _productDbContext.Products
                    .FirstOrDefaultAsync(p => p.ProductId == productId);
                if (product == null)
                {
                    _logger.LogWarning("Product with ID {ProductId} not found", productId);
                }

                var attribute = _mapper.Map<ProductAttribute>(createAttributeDto);
                attribute.ProductId = productId;
                attribute.Product = product; 
                attribute.CreatedAt = DateTime.UtcNow;
                attribute.UpdatedAt = DateTime.UtcNow;

                _productDbContext.ProductAttributes.Add(attribute);
                await _productDbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Attribute created successfully for Product ID: {ProductId}", productId);
                return _mapper.Map<ProductAttributeDto>(attribute);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating attribute");
                throw;
            }
        }

        public async Task<IEnumerable<ProductAttributeDto>> GetAllAttributesAsync()
        {
            try
            {
                var attributes = await _productDbContext.ProductAttributes.ToListAsync();
                return _mapper.Map<IEnumerable<ProductAttributeDto>>(attributes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving attributes");
                throw;
            }
        }

        public async Task<ProductAttributeDto> GetAttributeByIdAsync(int id)
        {
            try
            {
                var attribute = await _productDbContext.ProductAttributes.FirstOrDefaultAsync(a => a.ProductAttributeId == id);
                if (attribute == null) return null;
                return _mapper.Map<ProductAttributeDto>(attribute);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving attribute with ID {Id}", id);
                throw;
            }
        }

        public async Task<bool> UpdateAttributeAsync(int id, UpdateProductAttributeDto updateAttributeDto)
        {
            try
            {
                var attribute = await _productDbContext.ProductAttributes.FirstOrDefaultAsync(a => a.ProductAttributeId == id);
                if (attribute == null) return false;

                _mapper.Map(updateAttributeDto, attribute);
                attribute.UpdatedAt = DateTime.UtcNow;
                await _productDbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating attribute with ID {Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteAttributeAsync(int id)
        {
            try
            {
                var attribute = await _productDbContext.ProductAttributes.FirstOrDefaultAsync(a => a.ProductAttributeId == id);
                if (attribute == null) return false;

                _productDbContext.ProductAttributes.Remove(attribute);
                await _productDbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting attribute with ID {Id}", id);
                throw;
            }
        }

        
    }
}
