using AutoMapper;
using CoreBookingPlatform.ProductService.Models.DTOs;
using CoreBookingPlatform.ProductService.Models.Entities;

namespace CoreBookingPlatform.ProductService.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile() 
        {
            CreateMap<Product, ProductDto>()
                .ForMember(d => d.Categories, opt => opt.MapFrom(src => src.ProductCategories.Select(pc => pc.Category)))
                .ForMember(d => d.Attributes, opt => opt.MapFrom(src => src.Attributes))
                .ForMember(d => d.Contents, opt => opt.MapFrom(src => src.Contents))
                .ReverseMap()
                .ForMember(dest => dest.ProductCategories, opt => opt.Ignore()); 

            CreateMap<CreateProductDto, Product>();

            CreateMap<UpdateProductDto, Product>()
                .ForMember(dest => dest.ProductCategories, opt => opt.Ignore()); 

            CreateMap<Category, CategoryDto>();

            CreateMap<ProductContent, ProductContentDto>().ReverseMap();
            CreateMap<CreateProductContentDto, ProductContent>();
            CreateMap<UpdateProductContentDto, ProductContent>(); 

            CreateMap<ProductAttribute, ProductAttributeDto>();
            CreateMap<CreateProductAttributeDto, ProductAttribute>();
            CreateMap<UpdateProductAttributeDto, ProductAttribute>();

            CreateMap<UpdateCategoryDto, Category>();
            CreateMap<CreateCategoryDto, Category>();
           



        }

    }
}
