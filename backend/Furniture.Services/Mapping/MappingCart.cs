using AutoMapper;
using Furniture.Domain.Models;
using Furniture.shared.Dtos;

namespace Furniture.Services.Mapping
{
    public class MappingCart : Profile
    {
        public MappingCart()
        {
            CreateMap<CartItem, CartItemDto>()
                .ForMember(dest => dest.ProductName,
                    opt => opt.MapFrom(src => src.Product.NameEn))
                .ForMember(dest => dest.ProductImage,
                    opt => opt.MapFrom(src => src.Product.Images.FirstOrDefault().ImageUrl))
                .ForMember(dest => dest.SubTotal,
                    opt => opt.MapFrom(src => src.UnitPrice * src.Quantity))
                .ForMember(dest => dest.AvailableStock,
                    opt => opt.MapFrom(src => src.Product.StockQuantity));

          
            CreateMap<Cart, CartDto>()
                .ForMember(dest => dest.Items,
                    opt => opt.MapFrom(src => src.CartItems))
                .ForMember(dest => dest.TotalPrice,
                    opt => opt.MapFrom(src =>
                        src.CartItems.Sum(ci => ci.UnitPrice * ci.Quantity)))
                .ForMember(dest => dest.TotalItems,
                    opt => opt.MapFrom(src =>
                        src.CartItems.Sum(ci => ci.Quantity)));
        }
    }
}

