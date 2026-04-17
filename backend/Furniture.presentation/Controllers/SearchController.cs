using Furniture.Servises_Abstraction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Furniture.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;

        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> SearchByImage(
            IFormFile image,
            [FromQuery] int topK = 5)
        {
            if (image == null || image.Length == 0)
                return BadRequest("No image uploaded.");

            try
            {
                var results = await _searchService.SearchByImageAsync(image, topK);
                return Ok(results);
            }
            catch (Exception ex) when (ex.Message.Contains("unavailable"))
            {
                return StatusCode(503, "Search service unavailable.");
            }
        }

        
        [HttpPost("build-index")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> BuildIndex()
        {
            try
            {
                var result = await _searchService.BuildIndexAsync();
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex) when (ex.Message.Contains("unavailable") || ex.Message.Contains("Python"))
            {
                return StatusCode(503, ex.Message);
            }
        }
    }
}