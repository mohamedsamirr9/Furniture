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
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.UserId, o => o.MapFrom(s => s.UserId))
                .ForMember(d => d.UserName, o => o.MapFrom(s => s.User.UserName ?? s.User.Email))
                .ForMember(d => d.SellerId, o => o.MapFrom(s =>
                    s.Order.Offer != null
                        ? s.Order.Offer.SellerId
                        : s.Order.OrderItems!.Select(oi => oi.SellerId).FirstOrDefault()))
                .ForMember(d => d.ProductId, o => o.MapFrom(s =>
                    s.Order.OrderItems!.Select(oi => (int?)oi.ProductId).FirstOrDefault()))
                .ForMember(d => d.LatestReplyMessage, o => o.MapFrom(s =>
                    s.Replies.OrderByDescending(r => r.CreatedAt).Select(r => r.Message).FirstOrDefault()))
                .ForMember(d => d.LatestReplyBy, o => o.MapFrom(s =>
                    s.Replies.OrderByDescending(r => r.CreatedAt).Select(r => r.Responder.UserName ?? r.Responder.Email).FirstOrDefault()))
                .ForMember(d => d.LatestReplyAt, o => o.MapFrom(s =>
                    s.Replies.OrderByDescending(r => r.CreatedAt).Select(r => (DateTime?)r.CreatedAt).FirstOrDefault()));
            //Details 
            CreateMap<Complaint, ComplaintDetailDto>()
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.UserId, o => o.MapFrom(s => s.UserId))
                .ForMember(d => d.UserName, o => o.MapFrom(s => s.User.UserName))
                .ForMember(d => d.SellerId, o => o.MapFrom(s =>
                    s.Order.Offer != null
                        ? s.Order.Offer.SellerId
                        : s.Order.OrderItems!.Select(oi => oi.SellerId).FirstOrDefault()))
                .ForMember(d => d.SellerName, o => o.MapFrom(s =>
                    s.Order.Offer != null
                        ? s.Order.Offer.Seller.Name
                        : s.Order.OrderItems!.Select(oi => oi.Seller.Name).FirstOrDefault()))
                .ForMember(d => d.ProductId, o => o.MapFrom(s =>
                    s.Order.OrderItems!.Select(oi => (int?)oi.ProductId).FirstOrDefault()));

            CreateMap<ComplaintReply, ComplaintReplyDto>()
                .ForMember(d => d.ResponderName, o => o.MapFrom(s => s.Responder.UserName ?? s.Responder.Email));
            //Create
            CreateMap<ComplaintCreateDto, Complaint>()
                .ForMember(d => d.Status, o => o.MapFrom(s => ComplaintStatus.Open))
                .ForMember(d => d.CreatedAt, o => o.MapFrom(s => DateTime.UtcNow));

        }
    }
}
