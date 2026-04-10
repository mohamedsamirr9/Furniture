using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos.ProductDtos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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


        // POST: api/product
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductCreateUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _productService.CreateAsync(dto, GetLanguage());

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Id },
                result
            );
        }

        // PUT: api/product/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductCreateUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _productService.UpdateAsync(id, dto);

            return NoContent();
        }

        // DELETE: api/product/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _productService.DeleteAsync(id);

            return NoContent();
        }


    }
}
