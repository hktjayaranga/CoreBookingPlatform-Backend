using CoreBookingPlatform.ProductService.Models.DTOs;

namespace CoreBookingPlatform.ProductService.Services.Interfaces
{
    public interface IAttributeService
    {
        Task<ProductAttributeDto> CreateAttributeAsync(int productId, CreateProductAttributeDto createAttributeDto);
        Task<IEnumerable<ProductAttributeDto>> GetAllAttributesAsync();
        Task<ProductAttributeDto> GetAttributeByIdAsync(int id);
        Task<bool> UpdateAttributeAsync(int id, UpdateProductAttributeDto updateAttributeDto);
        Task<bool> DeleteAttributeAsync(int id);
    }
}
