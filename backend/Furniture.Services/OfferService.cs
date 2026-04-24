using AutoMapper;
using Furniture.Domain.InterfacesRepositories;
using Furniture.Domain.Models;
using Furniture.Domain.Models.Enum;
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

        public async Task<OfferDto?> GetOfferByIdAsync(int offerId)
        {
            var repo = _unitOfWork.GetRepository<Offer, int>();
            var offer = await repo.GetByIdAsync(offerId);
            return _mapper.Map<OfferDto>(offer);
        }

        public async Task AcceptOfferAsync(int offerId)
        {
            var repo = _unitOfWork.GetRepository<Offer, int>();
            var offer = await repo.GetByIdAsync(offerId);
            if (offer == null) throw new Exception("Offer not found");

            if (offer.Status != OfferStatus.Pending)
                throw new Exception("This offer has already been processed.");

            var allOffers = await repo.GetAllAsync(new OffersByRequestSpecification(offer.CustomRequestId));
            
            if (allOffers.Any(o => o.Status == OfferStatus.Accepted))
                throw new Exception("There is already an accepted offer for this request.");

            // Update the selected offer
            offer.Status = OfferStatus.Accepted;
            
            // Mark all other offers for this request as rejected
            foreach (var o in allOffers.Where(x => x.Id != offerId))
            {
                o.Status = OfferStatus.Rejected;
            }

            // Update the status of the parent Custom Request
            var requestRepo = _unitOfWork.GetRepository<CustomRequest, int>();
            var request = await requestRepo.GetByIdAsync(offer.CustomRequestId);
            if (request != null)
            {
                request.Status = CustomRequestStatus.Accepted;
            }

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
