using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos.SellerDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Furniture.presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SellersController : ControllerBase
    {
        private readonly ISellerService _sellerService;

        public SellersController(ISellerService sellerService)
        {
            _sellerService = sellerService;
        }

        private string GetLanguage() =>
            Request.Headers["Accept-Language"].FirstOrDefault()?.Trim() ?? "en";

        [Authorize(Roles = "seller")]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var seller = await _sellerService.GetSellerProfileForCurrentUserAsync(userId, GetLanguage());
            if (seller is null)
                return NotFound(new { message = "Seller profile not found." });

            return Ok(seller);
        }

        [Authorize(Roles = "seller")]
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateSellerProfileDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var ok = await _sellerService.UpdateSellerProfileAsync(userId, dto);
            if (!ok)
                return BadRequest(new { message = "Unable to update profile." });

            var updated = await _sellerService.GetSellerProfileForCurrentUserAsync(userId, GetLanguage());
            return Ok(updated);
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var seller = await _sellerService.GetSellerProfileByIdAsync(id, GetLanguage());

            if (seller is null)
            {
                return NotFound($"Seller with id {id} was not found.");
            }

            return Ok(seller);
        }
    }
}
