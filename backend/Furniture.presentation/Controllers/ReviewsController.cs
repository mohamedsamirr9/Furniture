using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.presentation.Controllers
{
    [ApiController]
    [Authorize]
    public class ReviewsController(IReviewService _reviewService) : ControllerBase
    {

        // GET /api/products/{productId}/reviews
        [HttpGet("api/products/{productId:int}/reviews")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ReviewDto>>> GetProductReviews(int productId, int pageIndex = 1, int pageSize = 10)
        {
            var reviews = await _reviewService.GetProductReviewsAsync(productId, pageIndex, pageSize);
            return Ok(reviews);
        }

        // GET /api/reviews/my/products
        [HttpGet("api/reviews/my/products")]
        [Authorize(Roles = "buyer")]
        public async Task<ActionResult<IEnumerable<int>>> GetMyReviewedProductIds()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
        return Unauthorized();
            var productIds = await _reviewService.GetUserReviewedProductIdsAsync(userId);
            return Ok(productIds);
        }

        // POST /api/reviews
        [HttpPost("api/reviews")]
        [Authorize(Roles = "buyer")]
        public async Task<ActionResult<ReviewDto>> CreateReview(ReviewCreateDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
             if (string.IsNullOrEmpty(userId))
        return Unauthorized();
            dto.UserId = userId;
            var review = await _reviewService.CreateReviewAsync(dto);
            return CreatedAtAction(null, new { id = review.Id }, review);
        }

        // DELETE /api/reviews/{id}
        [HttpDelete("api/reviews/{id:int}")]
        [Authorize(Roles = "buyer,admin")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            await _reviewService.DeleteReviewAsync(id);
            return NoContent();
        }
    }
}