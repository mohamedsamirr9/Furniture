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
    [Route("api/favorites")]
    public class FavouritesController : ControllerBase
    {
        private readonly IFavouriteService _favouriteService;

        public FavouritesController(IFavouriteService favouriteService)
        {
            _favouriteService = favouriteService;
        }


        // GET /api/favorites/{userId}
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetFavourites(string userId)
        {
            var favourites = await _favouriteService.GetFavouritesAsync(userId);
            return Ok(favourites);
        }


        // POST /api/favorites/{userId}/{productId}
        [HttpPost("{userId}/{productId}")]
        public async Task<IActionResult> AddToFavourites(string userId, int productId)
        {
            try
            {
                var favourite = await _favouriteService.AddToFavouritesAsync(userId, productId);
                return Ok(favourite);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
         
        }


        // DELETE /api/favorites/{userId}/{productId}
        [HttpDelete("{userId}/{productId}")]
        public async Task<IActionResult> RemoveFromFavourites(string userId, int productId)
        {
            try
            {
                await _favouriteService.RemoveFromFavouritesAsync(userId, productId);
                return Ok(new { message = "Product Removed From Favourites" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }
    }
}
