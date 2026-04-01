using AutoMapper;
using Furniture.Domain.Models;
using Furniture.shared.Dtos;
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

            CreateMap<Category, CategoryDto>();
            CreateMap<Category, CategoryListDto>();
            CreateMap<CategoryCreateUpdateDto, Category>()
                .ForMember(dest => dest.Created_At, opt => opt.MapFrom(d=> DateTime.UtcNow));
            CreateMap<Category, CategoryCreateUpdateDto>();
        }
    }
}
