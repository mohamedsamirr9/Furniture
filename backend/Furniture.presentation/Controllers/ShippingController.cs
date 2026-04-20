using Furniture.Servises_Abstraction;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Furniture.presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShippingController : ControllerBase
    {
        private readonly IShippingCalculatorService _shippingCalculator;

        public ShippingController(IShippingCalculatorService shippingCalculator)
        {
            _shippingCalculator = shippingCalculator;
        }

        [HttpGet("estimate")]
        public async Task<IActionResult> EstimateShipping([FromQuery] string city, [FromQuery] int? offerId)
        {
            if (string.IsNullOrWhiteSpace(city))
                return BadRequest("City is required.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) userId = "seller-1";

            var shippingCost = await _shippingCalculator.GetShippingEstimateAsync(userId, city, offerId);
            return Ok(new { shippingCost });
        }
    }
}

