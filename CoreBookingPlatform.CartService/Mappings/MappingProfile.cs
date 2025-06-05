using AutoMapper;
using CoreBookingPlatform.CartService.Models.DTOs;
using CoreBookingPlatform.CartService.Models.Entities;

namespace CoreBookingPlatform.CartService.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile() 
        {
            CreateMap<Cart, CartDto>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));
            CreateMap<CartItem,  CartItemDto>();
        }
    }
}
