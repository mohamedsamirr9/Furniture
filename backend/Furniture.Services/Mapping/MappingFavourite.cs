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
            CreateMap<Favourite, FavouriteDto>().ForMember(dest => dest.FavouriteId,opt => opt.MapFrom(src => src.Id))
                .ForMember(R => R.ProductId, b => b.MapFrom(src => src.Product.Id))

              .ForMember(R => R.ProductName, b => b.MapFrom(src => src.Product.Name))
                  .ForMember(R => R.ProductPrice, b => b.MapFrom(src => src.Product.Price))

               .ForMember(R => R.CategoryName, b => b.MapFrom(src => src.Product.Category.Name))
                    .ForMember(R => R.SellerName, b => b.MapFrom(src => src.Product.Seller.Name))

                 .ForMember(R => R.MainImage, b => b.MapFrom(src => src.Product.Images.FirstOrDefault().ImageUrl))
                   .ForMember(R => R.IsAvailable, b => b.MapFrom(src => src.Product.IsAvailable));
        }
    }
}
