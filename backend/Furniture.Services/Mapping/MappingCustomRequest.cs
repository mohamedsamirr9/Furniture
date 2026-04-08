using AutoMapper;
using Furniture.Domain.Models;
using Furniture.Domain.Models.Enum;
using Furniture.shared.Dtos.CustomRequestDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Services.Mapping
{
    public class MappingCustomRequest: Profile
    {
        public MappingCustomRequest()
        {
            // List
            CreateMap<CustomRequest, CustomRequestDto>()
                .ForMember(d=>d.Status, o=>o.MapFrom(s=>s.Status.ToString()))
                .ForMember(d=>d.BuyerName, o=>o.MapFrom(s=>s.Buyer.UserName))
                .ForMember(d=>d.AcceptedPrice, o=>o.MapFrom(s=>s.Offers.FirstOrDefault(o=>o.Status == OfferStatus.Accepted).Price));

            // Details
            CreateMap<CustomRequest, CustomRequestDetailsDto>()
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.BuyerName, o => o.MapFrom(s => s.Buyer.UserName))
                .ForMember(d=>d.Offers, o=>o.MapFrom(s=>s.Offers));

            // Create
            CreateMap<CustomRequestCreateDto, CustomRequest>()
                .ForMember(d => d.Status, o => o.MapFrom(s => CustomRequestStatus.Open));
        }
    }
}
