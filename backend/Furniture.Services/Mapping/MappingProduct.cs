using AutoMapper;
using Furniture.Domain.Models;
using Furniture.shared.Dtos.ProductDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Services.Mapping
{
    public class MappingProduct : Profile
    {
        public MappingProduct()
        {

            CreateMap<ProductCreateUpdateDto, Product>();

            CreateMap<Product, ProductListDto>().ForMember(d => d.CategoryName,o => o.MapFrom(s => s.Category.NameEn))
                           .ForMember(d => d.SellerName,o => o.MapFrom(s => s.Seller.UserName ?? s.Seller.Name ?? s.Seller.Email ?? string.Empty))
                           .ForMember(d => d.MainImage, o => o.MapFrom
                     (s => s.Images.FirstOrDefault() != null ? s.Images.FirstOrDefault()!.ImageUrl : null ));


            CreateMap<Product, ProductDetailsDto>().ForMember(d => d.CategoryName,o => o.MapFrom(s => s.Category.NameEn))
                          .ForMember(d => d.SellerId, o => o.MapFrom(s => s.SellerId))
                          .ForMember(d => d.SellerName, o => o.MapFrom(s => s.Seller.UserName ?? s.Seller.Name ?? s.Seller.Email ?? string.Empty))
                          .ForMember(d => d.Images,o => o.MapFrom(s =>  s.Images.Select(i => i.ImageUrl)));
        }
    }
}
