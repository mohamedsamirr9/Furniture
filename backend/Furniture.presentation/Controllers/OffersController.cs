
using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Furniture.API.Controllers
{
    [ApiController]
    [Route("api/offers")]
    public class OffersController : ControllerBase
    {
        private readonly IOfferService _offerService;

        public OffersController(IOfferService offerService)
        {
            _offerService = offerService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOffer([FromBody] OfferCreateDto dto)
        {
            var sellerId = "seller-1"; 
            var offer = await _offerService.CreateOfferAsync(dto, sellerId);
            return Ok(offer);
        }

        [HttpGet("request/{requestId}")]
        public async Task<IActionResult> GetOffersByRequest(int requestId)
        {
            var offers = await _offerService.GetOffersByRequestAsync(requestId);
            return Ok(offers);
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyOffers()
        {
            var sellerId = "seller-1"; 
            var offers = await _offerService.GetMyOffersAsync(sellerId);
            return Ok(offers);
        }

        [HttpPost("{id}/accept")]
        public async Task<IActionResult> AcceptOffer(int id)
        {
            await _offerService.AcceptOfferAsync(id);
            return Ok();
        }
    }
}