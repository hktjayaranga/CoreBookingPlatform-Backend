using AutoMapper;
using CoreBookingPlatform.OrderService.Models.DTOs;
using CoreBookingPlatform.OrderService.Models.Entities;

namespace CoreBookingPlatform.OrderService.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Order, OrderDto>();
            CreateMap<OrderItem, OrderItemDto>();
        }
    }
}
