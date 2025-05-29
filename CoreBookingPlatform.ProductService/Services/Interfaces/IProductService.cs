using CoreBookingPlatform.ProductService.Models.DTOs;

namespace CoreBookingPlatform.ProductService.Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllProductsAsync();
        Task<ProductDto> GetProductByIdAsync(int id);
        Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto);
        Task<bool> UpdateProductAsync(int id, UpdateProductDto updateProductDto);
        Task<bool> UpdateProductCoreAsync(int id, UpdateProductCoreDto dto);
        Task<bool> DeleteProductAsync(int id);
        Task<ProductContentDto> CreateProductContentAsync(CreateProductContentDto createContentDto);
        Task<ProductContentDto> GetProductContentByIdAsync(int id);
        Task<bool> UpdateProductContentAsync(int id, UpdateProductContentDto updateContentDto);
        Task<bool> DeleteProductContentAsync(int id);
    }
}
