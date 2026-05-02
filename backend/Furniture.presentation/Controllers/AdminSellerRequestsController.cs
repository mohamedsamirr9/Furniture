using Furniture.Domain.Models.Enum;
using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos.SellerRequest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Furniture.web;

[Route("api/admin/seller-requests")]
[ApiController]
[Authorize(Roles = "admin")]
public class AdminSellerRequestsController(ISellerRequestService sellerRequestService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SellerRequestDto>>> GetList([FromQuery] string? status)
    {
        if (!TryParseStatus(status, out var parsed))
            return BadRequest(new { message = "Invalid status. Use Pending, Approved, or Rejected." });

        var list = await sellerRequestService.GetSellerRequestsForAdminAsync(parsed);
        return Ok(list);
    }

    private static bool TryParseStatus(string? status, out SellerRequestStatus parsed)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            parsed = SellerRequestStatus.Pending;
            return true;
        }

        return Enum.TryParse(status, ignoreCase: true, out parsed)
               && Enum.IsDefined(typeof(SellerRequestStatus), parsed);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SellerRequestDto>> GetById(int id)
    {
        var dto = await sellerRequestService.GetByIdAsync(id);
        if (dto is null)
            return NotFound();
        return Ok(dto);
    }

    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id)
    {
        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminId))
            return Unauthorized();

        await sellerRequestService.ApproveAsync(id, adminId);
        return NoContent();
    }

    [HttpPost("{id:int}/reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] RejectSellerRequestDto dto)
    {
        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminId))
            return Unauthorized();

        await sellerRequestService.RejectAsync(id, adminId, dto);
        return NoContent();
    }
}
