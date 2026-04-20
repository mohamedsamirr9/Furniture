using AutoMapper;
using Furniture.Domain.Models;
using Furniture.shared.Dtos.AuthDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Services.Mapping
{
    public class MappingUser :Profile
    {
        public MappingUser()
        {
            CreateMap<ApplicationUser, UserDto>();

            CreateMap<RegisterDto, ApplicationUser>()
     .ForMember(dest => dest.NationalIdImage, opt => opt.Ignore())
     .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName.Trim()))
     .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.ToLower().Trim()))
     // Ignore IdentityUser internal fields
     .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
     .ForMember(dest => dest.SecurityStamp, opt => opt.Ignore())
     .ForMember(dest => dest.ConcurrencyStamp, opt => opt.Ignore())
     .ForMember(dest => dest.NormalizedEmail, opt => opt.Ignore())
     .ForMember(dest => dest.NormalizedUserName, opt => opt.Ignore())
     .ForMember(dest => dest.LockoutEnd, opt => opt.Ignore())
     .ForMember(dest => dest.RefreshTokens, opt => opt.Ignore())
     .ForMember(dest => dest.Products, opt => opt.Ignore())
     .ForMember(dest => dest.Cart, opt => opt.Ignore());
        }
    }
}
