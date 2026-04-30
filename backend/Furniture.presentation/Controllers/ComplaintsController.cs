using Furniture.Domain.Models;
using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos.ComplaintsDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Furniture.web
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ComplaintsController(IComplaintService _complaintService) : ControllerBase
    {
        [HttpGet]
        [Authorize(Roles ="admin")]
        public async Task<ActionResult<IEnumerable<ComplaintDto>>> GetAll(string? status = null)
        {
            var result = await _complaintService.GetAllAsync(status);
            return Ok(result);
        }
                [Authorize (Roles ="buyer")]
        [HttpGet("My")]
        public async Task<ActionResult<IEnumerable<ComplaintDto>>> GetMy()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            //var userId = "seller-1";
            var result = await _complaintService.GetMyAsync(userId!);
            return Ok(result);
        }

        [HttpGet("seller")]
        [Authorize(Roles = "seller")]
        public async Task<ActionResult<IEnumerable<ComplaintDto>>> GetSellerComplaints()
        {
            var sellerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _complaintService.GetSellerComplaintsAsync(sellerId!);
            return Ok(result);
        }
        
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ComplaintDetailDto>> Get(int id) 
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(role)) return Unauthorized();
            var result= await _complaintService.GetByIdAsync(id, userId, role);
            return Ok(result);
        }
        [Authorize (Roles ="buyer")]
        [HttpPost]
        public async Task<ActionResult<ComplaintDto>> Create([FromBody] ComplaintCreateDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            //var userId = "seller-1";
            if (userId == null) return Unauthorized();

            var result = await _complaintService.CreateAsync(userId, dto);
            return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
        }
        [Authorize (Roles ="admin, buyer")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] ComplaintCreateDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            //var userId = "seller-1";
            if (userId == null) return Unauthorized();

            await _complaintService.UpdateAsync(id, userId, dto);
            return NoContent();
        }
        [Authorize(Roles = "admin, buyer")]
        [HttpPost("{id:int}/close")]
        public async Task<IActionResult> Close(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrWhiteSpace(role)) return Unauthorized();
            await _complaintService.CloseAsync(id, userId, role);
            return NoContent();
        }

        [Authorize(Roles = "seller,admin")]
        [HttpPatch("{id:int}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateComplaintStatusDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(role)) return Unauthorized();

            await _complaintService.UpdateStatusAsync(id, userId, role, dto);
            return NoContent();
        }

        [Authorize(Roles = "seller,admin")]
        [HttpPost("{id:int}/replies")]
        public async Task<ActionResult<ComplaintReplyDto>> Reply(int id, [FromBody] ReplyComplaintDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(role)) return Unauthorized();

            var response = await _complaintService.ReplyAsync(id, userId, role, dto);
            return Ok(response);
        }

        [Authorize(Roles = "seller,admin")]
        [HttpGet("{id:int}/replies")]
        public async Task<ActionResult<IEnumerable<ComplaintReplyDto>>> GetReplies(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(role)) return Unauthorized();

            var detail = await _complaintService.GetByIdAsync(id, userId, role);
            return Ok(detail.Replies);
        }

        [Authorize(Roles = "admin,seller,buyer")]
        [HttpGet("{id:int}/detail")]
        public async Task<ActionResult<ComplaintDetailDto>> GetDetail(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(role)) return Unauthorized();

            var result = await _complaintService.GetByIdAsync(id, userId, role);
            return Ok(result);
        }

        [Authorize(Roles = "admin,seller,buyer")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(role)) return Unauthorized();

            await _complaintService.DeleteAsync(id, userId, role);
            return NoContent();
        }
    }
}


   