using AutoMapper;
using Furniture.Domain.Models;
using Furniture.shared.Dtos.Order;

namespace Furniture.Services.Mappings
{
    public class MappingOrder : Profile
    {
        public MappingOrder()
        {
            CreateMap<Order, OrderDTO>()
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.PaymentMethod,
                    opt => opt.MapFrom(src => src.Payment != null ? src.Payment.Method.ToString() : "Cash"))
                .ForMember(dest => dest.UserName,
                    opt => opt.MapFrom(src => src.User != null ? src.User.UserName : null))
                .ForMember(dest => dest.OrderItems,
                    opt => opt.MapFrom(src => src.OrderItems));

            CreateMap<OrderItem, OrderItemDTO>()
                .ForMember(dest => dest.ProductName,
                    opt => opt.MapFrom(src => src.Product != null ? src.Product.NameEn : null))
                .ForMember(dest => dest.ProductImage,
                    opt => opt.MapFrom(src => src.Product != null && src.Product.Images != null && src.Product.Images.Any()
                        ? src.Product.Images.First().ImageUrl
                        : null));
        }
    }
}