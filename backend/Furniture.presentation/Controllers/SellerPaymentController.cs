using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos.Seller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Furniture.presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SellersController : ControllerBase
    {
        private readonly ISellerPaymentService _sellerPaymentService;

        public SellersController(ISellerPaymentService sellerPaymentService)
        {
            _sellerPaymentService = sellerPaymentService;
        }

        // ============================================
        // Seller Endpoints
        // ============================================

        /// <summary>
        /// Seller يسجل بيانات متجره
        /// POST: api/sellers/profile
        /// </summary>
        [HttpPost("profile")]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> CreateProfile(
            [FromBody] CreateSellerProfileDTO dto)
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

        /// <summary>
        /// Seller يشوف بيانات متجره
        /// GET: api/sellers/profile
        /// </summary>
        [HttpGet("profile")]
        [Authorize(Roles = "Seller")]
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

        /// <summary>
        /// Seller يشوف أرباحه
        /// GET: api/sellers/earnings
        /// </summary>
        [HttpGet("earnings")]
        [Authorize(Roles = "Seller")]
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

        // ============================================
        // Admin Endpoints
        // ============================================

        /// <summary>
        /// Admin يشوف كل الـ Sellers
        /// GET: api/sellers/admin/all
        /// </summary>
        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllSellers()
        {
            var result = await _sellerPaymentService.GetAllSellersAsync();
            return Ok(result);
        }

        /// <summary>
        /// Admin يشوف Sellers في انتظار الموافقة
        /// GET: api/sellers/admin/pending
        /// </summary>
        [HttpGet("admin/pending")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPendingSellers()
        {
            var result = await _sellerPaymentService.GetPendingSellersAsync();
            return Ok(result);
        }

        /// <summary>
        /// Admin يوافق على Seller
        /// PUT: api/sellers/admin/5/verify
        /// </summary>
        [HttpPut("admin/{sellerId:int}/verify")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> VerifySeller(int sellerId)
        {
            try
            {
                var result = await _sellerPaymentService.VerifySellerAsync(sellerId);

                if (!result)
                    return NotFound(new { message = "Seller not found" });

                return Ok(new { message = "Seller verified and Sub-merchant created!" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}