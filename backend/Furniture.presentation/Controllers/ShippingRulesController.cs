using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos.ShippingRule;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Furniture.presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShippingRulesController(IShippingService _shippingService) : ControllerBase
    {
        // GET /api/shippingrules?city=cairo&categoryId=1
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ShippingRuleDto>>> GetAll(string? city, int? categoryId)
        {
            var rules = await _shippingService.GetAllAsync(city, categoryId);
            return Ok(rules);
        }

        // GET /api/shippingrules/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ShippingRuleDto>> GetById(int id)
        {
            var rule = await _shippingService.GetByIdAsync(id);
            if (rule is null) return NotFound();
            return Ok(rule);
        }

        // POST /api/shippingrules
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<ShippingRuleDto>> Create(ShippingRuleCreateUpdateDto dto)
        {
            try
            {
                var rule = await _shippingService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = rule.Id }, rule);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT /api/shippingrules/{id}
        [HttpPut("{id:int}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Update(int id, ShippingRuleCreateUpdateDto dto)
        {
            try
            {
                await _shippingService.UpdateAsync(id, dto);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE /api/shippingrules/{id}
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int id)
        {
            await _shippingService.DeleteAsync(id);
            return NoContent();
        }
    }
}