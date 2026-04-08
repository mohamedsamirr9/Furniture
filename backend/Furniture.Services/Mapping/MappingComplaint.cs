using AutoMapper;
using Furniture.Domain.Models;
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
            //when you create a list
            CreateMap<Complaint, ComplaintDto>()
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));
            //details 
            CreateMap<Complaint, ComplaintDetails>()    


        }
    }
}
