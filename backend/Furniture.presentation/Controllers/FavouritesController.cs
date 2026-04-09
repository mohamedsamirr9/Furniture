using Furniture.Servises_Abstraction;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.presentation.Controllers
{
    [ApiController]
    [Route("api/favourites")]
    public class FavouritesController : ControllerBase
    {
        private readonly IFavouriteService _favouriteService;

        public FavouritesController(IFavouriteService favouriteService)
        {
            _favouriteService = favouriteService;
        }

        private string GetUserId() => "seller-1";

        // GET /api/favourites
        [HttpGet]
        public async Task<IActionResult> GetFavourites()
        {
            var favourites = await _favouriteService.GetFavouritesAsync(GetUserId());
            return Ok(favourites);
        }

        // POST /api/favourites/{productId}
        [HttpPost("{productId}")]
        public async Task<IActionResult> AddToFavourites(int productId)
        {
            try
            {
                var favourite = await _favouriteService.AddToFavouritesAsync(GetUserId(), productId);
                return Ok(favourite);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // DELETE /api/favourites/{productId}
        [HttpDelete("{productId}")]
        public async Task<IActionResult> RemoveFromFavourites(int productId)
        {
            try
            {
                await _favouriteService.RemoveFromFavouritesAsync(GetUserId(), productId);
                return Ok(new { message = "Product Removed From Favourites" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }
    }
}
