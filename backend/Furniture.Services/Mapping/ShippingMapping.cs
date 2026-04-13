using AutoMapper;
using Furniture.Domain.Models;
using Furniture.shared.Dtos.ShippingRule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Services.Mapping
{
    public class ShippingMapping : Profile
    {
        public ShippingMapping()
        {
            CreateMap<ShippingRule, ShippingRuleDto>()
           .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category.NameEn));

            CreateMap<ShippingRuleCreateUpdateDto, ShippingRule>();
        }
    }
}
