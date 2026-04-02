using Furniture.shared.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Servises_Abstraction
{
    public interface IOfferService
    {
        Task<OfferDto> CreateOfferAsync(OfferCreateDto dto, string sellerId);
        Task<IEnumerable<OfferDto>> GetOffersByRequestAsync(int requestId);
        Task<IEnumerable<OfferDto>> GetMyOffersAsync(string sellerId);
        Task AcceptOfferAsync(int offerId);
    }
}
