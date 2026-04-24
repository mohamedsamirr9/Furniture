using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos.Seller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Furniture.presentation.Controllers
{
    [ApiController]
    [Route("api/sellers")]
    [Authorize]
    public class SellerPaymentsController : ControllerBase
    {
        private readonly ISellerPaymentService _sellerPaymentService;

        public SellerPaymentsController(ISellerPaymentService sellerPaymentService)
        {
            _sellerPaymentService = sellerPaymentService;
        }


        [HttpPost("profile")]
        [Authorize(Roles = "seller")]
        public async Task<IActionResult> CreateProfile([FromBody] CreateSellerProfileDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _sellerPaymentService.CreateSellerProfileAsync(userId, dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("profile")]
        [Authorize(Roles = "seller")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _sellerPaymentService.GetMyProfileAsync(userId);

            if (result == null)
                return NotFound(new { message = "Profile not found" });

            return Ok(result);
        }

        [HttpGet("earnings")]
        [Authorize(Roles = "seller")]
        public async Task<IActionResult> GetEarnings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _sellerPaymentService.GetEarningsAsync(userId);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        

        [HttpGet("admin/all")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAllSellers()
        {
            var result = await _sellerPaymentService.GetAllSellersAsync();
            return Ok(result);
        }

        [HttpGet("admin/pending")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetPendingSellers()
        {
            var result = await _sellerPaymentService.GetPendingSellersAsync();
            return Ok(result);
        }

        [HttpPut("admin/{sellerId:int}/verify")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> VerifySeller(int sellerId)
        {
            try
            {
                var result = await _sellerPaymentService.VerifySellerAsync(sellerId);

                if (!result)
                    return NotFound(new { message = "Seller not found" });

                return Ok(new { message = "Seller verified successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("admin/payouts/{payoutId:int}/retry")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> RetryPayout(int payoutId)
        {
            try
            {
                var result = await _sellerPaymentService.RetryFailedPayoutAsync(payoutId);

                if (!result)
                    return BadRequest(new { message = "Payout not found or not in failed state" });

                return Ok(new { message = "Payout retried successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}