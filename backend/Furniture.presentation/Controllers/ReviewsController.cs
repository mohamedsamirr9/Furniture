using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.presentation.Controllers
{
    [ApiController]
    public class ReviewsController(IReviewService _reviewService) : ControllerBase
    {
        // GET /api/products/{productId}/reviews
        [HttpGet("api/products/{productId:int}/reviews")]
        public async Task<ActionResult<IEnumerable<ReviewDto>>> GetProductReviews(int productId, int pageIndex = 1, int pageSize = 10)
        {
            var reviews = await _reviewService.GetProductReviewsAsync(productId, pageIndex, pageSize);
            return Ok(reviews);
        }

        // POST /api/reviews
        [HttpPost("api/reviews")]
        public async Task<ActionResult<ReviewDto>> CreateReview(ReviewCreateDto dto)
        {
            var review = await _reviewService.CreateReviewAsync(dto);
            return CreatedAtAction(null, new { id = review.Id }, review);
        }

        // DELETE /api/reviews/{id}
        [HttpDelete("api/reviews/{id:int}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            await _reviewService.DeleteReviewAsync(id);
            return NoContent();
        }
    }
}