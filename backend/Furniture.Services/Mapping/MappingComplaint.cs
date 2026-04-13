using AutoMapper;
using Furniture.Domain.Models;
using Furniture.Domain.Models.Enum;
using Furniture.shared.Dtos.ComplaintsDto;
using System;   
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Services.Mapping
{
    public class MappingComplaint: Profile
    {
        public MappingComplaint()
        {
            //Listing
            CreateMap<Complaint, ComplaintDto>()
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));
            //Details 
            CreateMap<Complaint, ComplaintDetailDto>()
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.UserName, o => o.MapFrom(s => s.User.UserName));
            //Create
            CreateMap<ComplaintCreateDto, Complaint>()
                .ForMember(d => d.Status, o => o.MapFrom(s => ComplaintStatus.Open))
                .ForMember(d => d.CreatedAt, o => o.MapFrom(s => DateTime.UtcNow));

        }
    }
}
