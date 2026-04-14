using AutoMapper;
using Furniture.Domain.Models;
using Furniture.shared.Dtos.FavouriteProductDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Services.Mapping
{
    public class MappingFavourite : Profile
    {
        public MappingFavourite()
        {
            CreateMap<Favourite, FavouriteDto>()
                .ForMember(d => d.FavouriteId, o => o.MapFrom(s => s.Id))
                .ForMember(d => d.ProductId, o => o.MapFrom(s => s.Product.Id))
                .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product.NameEn))
                .ForMember(d => d.ProductPrice, o => o.MapFrom(s => s.Product.Price))
                .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Product.Category.NameEn))
                .ForMember(d => d.SellerName, o => o.MapFrom(s => s.Product.Seller.Name))
                .ForMember(d => d.MainImage, o => o.MapFrom(s => s.Product.Images.FirstOrDefault() == null ? string.Empty : s.Product.Images.FirstOrDefault()!.ImageUrl))
                .ForMember(d => d.IsAvailable, o => o.MapFrom(s => s.Product.IsAvailable));
        }
    }
}
