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
        [Authorize(Roles ="Admin")]
        public async Task<ActionResult<IEnumerable<ComplaintDto>>> GetAll(string? status = null)
        {
            var result = await _complaintService.GetAllAsync(status);
            return Ok(result);
        }
        [HttpGet("My")]
        public async Task<ActionResult<IEnumerable<ComplaintDto>>> GetMy()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            //var userId = "seller-1";
            var result = await _complaintService.GetMyAsync(userId!);
            return Ok(result);
        }
        
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ComplaintDetailDto>> Get(int id) 
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            //var userId = "seller-1";
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var result= await _complaintService.GetByIdAsync(id);
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
        [Authorize("admin, buyer")]
        [HttpPost("{id:int}/close")]
        public async Task<IActionResult> Close(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            //var userId = "seller-1";
            if (userId == null) return Unauthorized();
            var rule = User.FindFirst(ClaimTypes.Role)?.Value;
            await _complaintService.CloseAsync(id, userId);
            return NoContent();
        }
    }
}


   