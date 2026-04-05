using AutoMapper;
using Furniture.Domain.InterfacesRepositories;
using Furniture.Domain.Models;
using Furniture.Services.Specifications;
using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Services
{
    public class OfferService(IUnitOfWork _unitOfWork, IMapper _mapper) : IOfferService
    {
 
        public async Task<OfferDto> CreateOfferAsync(OfferCreateDto dto, string sellerId)
        {
            var offer = _mapper.Map<Offer>(dto);
            offer.SellerId = sellerId;

            var repo = _unitOfWork.GetRepository<Offer, int>();
            await repo.AddAsync(offer);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<OfferDto>(offer);
        }

        public async Task<IEnumerable<OfferDto>> GetOffersByRequestAsync(int requestId)
        {
            var spec = new OffersByRequestSpecification(requestId);
            var repo = _unitOfWork.GetRepository<Offer, int>();
            var offers = await repo.GetAllAsync(spec);
            return _mapper.Map<IEnumerable<OfferDto>>(offers);
        }

        public async Task<IEnumerable<OfferDto>> GetMyOffersAsync(string sellerId)
        {
            var spec = new MyOffersSpecification(sellerId);
            var repo = _unitOfWork.GetRepository<Offer, int>();
            var offers = await repo.GetAllAsync(spec);
            return _mapper.Map<IEnumerable<OfferDto>>(offers);
        }

        public async Task AcceptOfferAsync(int offerId)
        {
            var repo = _unitOfWork.GetRepository<Offer, int>();
            var offer = await repo.GetByIdAsync(offerId);
            if (offer == null) throw new Exception("Offer not found");

            var otherOffers = await repo.GetAllAsync(new OffersByRequestSpecification(offer.OrderRequestId));
            foreach (var o in otherOffers)
            {
                o.IsAccepted = o.Id == offerId;
            }

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
