using System.Security.Claims;
using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos.Recommendation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/recommendations")]
public class RecommendationController : ControllerBase
{
    private readonly IRecommendationService _recommendationService;

    public RecommendationController(IRecommendationService recommendationService)
        => _recommendationService = recommendationService;

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpPost("quiz")]
    [Authorize(Roles = "buyer")]
    public async Task<IActionResult> SaveQuiz([FromBody] QuizDto dto)
    {
        try
        {
            await _recommendationService.SaveUserQuizAsync(
                GetUserId(), dto.Style, dto.Color, dto.RoomSize, dto.Budget);
            return Ok(new { message = "Preferences saved!" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    [Authorize(Roles = "buyer")]
    public async Task<IActionResult> GetRecommendations([FromQuery] int topK = 5)
    {
        try
        {
            var result = await _recommendationService.GetRecommendationsAsync(GetUserId(), topK);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("action")]
    [Authorize(Roles = "buyer")]
    public async Task<IActionResult> TrackAction([FromBody] ActionDto dto)
    {
        try
        {
            await _recommendationService.UpdateUserEmbeddingAsync(
                GetUserId(), dto.ProductId, dto.ActionType);
            return Ok(new { message = "Preference updated." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("index-all")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> IndexAllProducts()
    {
        try
        {
            await _recommendationService.IndexAllProductsAsync();
            return Ok(new { message = "All products indexed." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    [HttpGet("quiz-status")]
    [Authorize(Roles = "buyer")]
    public async Task<ActionResult<QuizStatusDto>> GetQuizStatus()
    {
        var isCompleted = await _recommendationService.HasCompletedQuizAsync(GetUserId());

        return Ok(new QuizStatusDto
        {
            IsCompleted = isCompleted
        });
    }
}