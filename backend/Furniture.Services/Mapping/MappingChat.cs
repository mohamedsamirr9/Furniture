using AutoMapper;
using Furniture.Domain.Models;
using Furniture.shared.Dtos.ConversationDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Services.Mapping
{
    public class MappingChat : Profile
    {
        public MappingChat()
        {
            CreateMap<Message, MessageDto>()
                .ForMember(d => d.SenderName, o => o.MapFrom(s => s.Sender.Name));
        }
    }
}
