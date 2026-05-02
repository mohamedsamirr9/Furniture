using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos.SellerRequest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Furniture.web;

[Route("api/seller")]
[ApiController]
[Authorize]
public class SellerRequestController(ISellerRequestService sellerRequestService) : ControllerBase
{
    /// <summary>Current user's seller application (pending preferred, else latest).</summary>
    [HttpGet("my-request")]
    public async Task<ActionResult<SellerRequestDto?>> GetMyRequest()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var dto = await sellerRequestService.GetMyRequestAsync(userId);
        return Ok(dto);
    }
}
