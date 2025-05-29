using AutoMapper;
using CoreBookingPlatform.ProductService.Models.DTOs;
using CoreBookingPlatform.ProductService.Models.Entities;

namespace CoreBookingPlatform.ProductService.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile() 
        {


            //CreateMap<Product, ProductDto>()
            //.ForMember(d => d.Categories,
            //               opt => opt.MapFrom(src => src.ProductCategories.Select(pc => pc.Category)))
            //.ForMember(d => d.Attributes,
            //               opt => opt.MapFrom(src => src.Attributes))
            //.ForMember(d => d.Contents,
            //           opt => opt.MapFrom(src => src.Contents))
            //.ReverseMap()
            //.ForMember(dest => dest.ProductCategories, opt => opt.Ignore());

            //CreateMap<CreateProductDto, Product>();
            //CreateMap<UpdateProductDto, Product>()
            //    .ForMember(dest => dest.ProductCategories, opt => opt.Ignore())
            //    .ForMember(dest => dest.Attributes, opt => opt.Ignore())
            //    .ForMember(dest => dest.Contents, opt => opt.Ignore());

            //CreateMap<Category, CategoryDto>();
            //CreateMap<CreateProductDto, Product>()
            //    .ForMember(dest => dest.ProductCategories, opt => opt.Ignore())
            //    .ForMember(dest => dest.Attributes, opt => opt.Ignore())
            //    .ForMember(dest => dest.Contents, opt => opt.Ignore());
            //CreateMap<UpdateProductDto, Product>()
            //    .ForMember(dest => dest.ProductCategories, opt => opt.Ignore())
            //    .ForMember(dest => dest.Attributes, opt => opt.Ignore())
            //    .ForMember(dest => dest.Contents, opt => opt.Ignore());

            //CreateMap<ProductContent, ProductContentDto>().ReverseMap();
            //CreateMap<CreateProductContentDto, ProductContent>();
            //CreateMap<UpdateProductContentDto, ProductContent>()
            //.ForMember(dest => dest.ProductId, opt => opt.Ignore())
            //.ForMember(dest => dest.Product, opt => opt.Ignore())
            //.ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            //.ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            //CreateMap<ProductAttribute, ProductAttributeDto>();
            //CreateMap<CreateProductAttributeDto, ProductAttribute>()
            //.ForMember(dest => dest.ProductId, opt => opt.Ignore()) 
            //.ForMember(dest => dest.Product, opt => opt.Ignore())   
            //.ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) 
            //.ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            //CreateMap<UpdateProductAttributeDto, ProductAttribute>()
            //    .ForMember(dest => dest.ProductId, opt => opt.Ignore())
            //    .ForMember(dest => dest.Product, opt => opt.Ignore())
            //    .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            //    .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            //CreateMap<ProductContent, ProductContentDto>().ReverseMap();
            //CreateMap<CreateProductContentDto, ProductContent>();

            //CreateMap<UpdateCategoryDto, Category>();
            //CreateMap<CreateCategoryDto, Category>();
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
