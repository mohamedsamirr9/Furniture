using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos.CustomRequestDto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Furniture.web
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomRequestController(ICustomRequestService _customRequestService) : ControllerBase
    {
        // Get all with filters
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomRequestDto>>> GetAll(int pageIndex=1,  int pageSize=10, string? status=null, decimal? minBudget=null)
        {
            var requests =await _customRequestService.GetAllAsync(pageIndex, pageSize, status, minBudget);
            return Ok(requests);
        }

        // Get my requests
        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<CustomRequestDto>>> GetMyRequests()
        {
             var buyerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var requests= await _customRequestService.GetMyRequestsAsync(buyerId!);

            return Ok(requests);
        }

        //Get by id with details and offers
        [HttpGet("{id:int}")]
        public async Task<ActionResult<CustomRequestDto>> GetRequest(int id)
        {
            var request= await _customRequestService.GetByIdAsync(id);
            return Ok(request);
        }

        // Create request
        [HttpPost]
        public async Task<ActionResult<CustomRequestDto>> CreateRequest(CustomRequestCreateDto dto)
        {
            var buyerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var request= await _customRequestService.CreateAsync(buyerId!, dto);

            return CreatedAtAction(nameof(GetRequest), new { id=request.Id},  request);
        }

        // Update request
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateRequest(int id, CustomRequestCreateDto dto)
        {
            var buyerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await _customRequestService.UpdateAsync(id,buyerId! ,dto);
            return NoContent();
        }

        // Cancel request
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> CancelRequest(int id)
        {
            var buyerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await _customRequestService.CancelRequest(id, buyerId!);

            return NoContent();
        }

    }
}
