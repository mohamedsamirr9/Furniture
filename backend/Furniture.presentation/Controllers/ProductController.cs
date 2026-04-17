using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos.ProductDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Furniture.Servises_Abstraction.Exceptions;

namespace Furniture.presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        private string GetLanguage() =>
            Request.Headers["Accept-Language"].FirstOrDefault()?.Trim() ?? "en";

        // GET: api/product
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ProductQueryParams queryParams)
        {
            var result = await _productService.GetAllAsync(queryParams, GetLanguage());
            return Ok(result);
        }

        // GET: api/product/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _productService.GetByIdAsync(id, GetLanguage());

            if (result is null)
                return NotFound($"Product with id {id} not found");

            return Ok(result);
        }


        // // POST: api/product
        // [Authorize(Roles ="seller")]
        // [HttpPost]
        // public async Task<IActionResult> Create([FromBody] ProductCreateUpdateDto dto)
        // {
        //     if (!ModelState.IsValid)
        //         return BadRequest(ModelState);
        //
        //     var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //     if (string.IsNullOrEmpty(userId))
        //         return Unauthorized("User ID not found in claims");
        //
        //     dto.SellerId = userId;
        //
        //     var result = await _productService.CreateAsync(dto, GetLanguage());
        //
        //     return CreatedAtAction(
        //         nameof(GetById),
        //         new { id = result.Id },
        //         result
        //     );
        // }
        
        [Authorize(Roles = "seller")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductCreateUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized("User ID not found in claims");

            dto.SellerId = userId;

            try
            {
                var result = await _productService.CreateAsync(dto, GetLanguage());
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (ImageValidationException ex)
            {
                return BadRequest(new
                {
                    message = "Some images were rejected",
                    results = ex.Summary.Results.Select(r => new
                    {
                        url = r.Url,
                        decision = r.Decision,
                        aiProbability = r.AiProbability
                    })
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "seller")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductCreateUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized("User ID not found in claims");

            dto.SellerId = userId;

            try
            {
                await _productService.UpdateAsync(id, dto);
                return NoContent();
            }
            catch (ImageValidationException ex)
            {
                return BadRequest(new
                {
                    message = "Some images were rejected",
                    results = ex.Summary.Results.Select(r => new
                    {
                        url = r.Url,
                        decision = r.Decision,
                        aiProbability = r.AiProbability
                    })
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // // PUT: api/product/5
        // [Authorize(Roles ="seller")]
        // [HttpPut("{id}")]
        // public async Task<IActionResult> Update(int id, [FromBody] ProductCreateUpdateDto dto)
        // {
        //     if (!ModelState.IsValid)
        //         return BadRequest(ModelState);
        //
        //     var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //     if (string.IsNullOrEmpty(userId))
        //         return Unauthorized("User ID not found in claims");
        //
        //     dto.SellerId = userId;
        //
        //     await _productService.UpdateAsync(id, dto);
        //
        //     return NoContent();
        // }

        // DELETE: api/product/5
        [Authorize(Roles ="seller")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _productService.DeleteAsync(id);

            return NoContent();
        }


    }
}
