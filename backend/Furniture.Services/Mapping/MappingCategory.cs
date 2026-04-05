using AutoMapper;
using Furniture.Domain.Models;
using Furniture.shared.Dtos.CategoryDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Services.Mapping
{
    public class MappingCategory : Profile
    {
        public MappingCategory()
        {

            CreateMap<Category, CategoryDto>().ForMember(d => d.Products, o => o.MapFrom(s => s.Products)); ;
            CreateMap<Category, CategoryListDto>();
            CreateMap<CategoryCreateUpdateDto, Category>().ForMember(dest => dest.Created_At, opt => opt.MapFrom(d=> DateTime.UtcNow));
            CreateMap<Category, CategoryCreateUpdateDto>();
         
        }
    }
}
