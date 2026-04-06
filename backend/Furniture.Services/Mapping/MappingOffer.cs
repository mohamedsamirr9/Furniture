using AutoMapper;
using Furniture.Domain.Models;
using Furniture.Domain.Models.Enum;
using Furniture.shared.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Services.Mapping
{
    public class MappingOffer : Profile
    {
        public MappingOffer()
        {
            CreateMap<OfferCreateDto, Offer>()
            .ForMember(dest => dest.Status,
                       opt => opt.MapFrom(src => OfferStatus.Pending));

            CreateMap<Offer, OfferDto>();
        }
    }
}
