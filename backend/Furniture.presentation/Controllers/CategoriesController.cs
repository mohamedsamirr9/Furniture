using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos.CategoryDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController(ICategoryService _categoryService) : ControllerBase
    {
        private string GetLanguage() =>
            Request.Headers["Accept-Language"].FirstOrDefault()?.Trim() ?? "en";
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryListDto>>> GetAllCategories(int pageIndex = 1, int pageSize = 10, string? search = null)
        {
            var categories = await _categoryService.GetAllCategoriesAsync(pageIndex, pageSize, search, GetLanguage());
            return Ok(categories);
        }

        // GET /api/categories/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<CategoryDto>> GetCategory(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id, GetLanguage());
            return Ok(category);
        }

        // POST
        [Authorize(Roles ="admin, seller")]
        [HttpPost]
        public async Task<ActionResult<CategoryDto>> CreateCategory(CategoryCreateUpdateDto dto)
        {
            var category = await _categoryService.CreateCategoryAsync(dto, GetLanguage());

            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
        }

        // PUT
        [Authorize(Roles = "admin, seller")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateCategory(int id, CategoryCreateUpdateDto dto)
        {
            await _categoryService.UpdateCategoryAsync(id, dto);
            return NoContent();
        }

        // DELETE
        [Authorize(Roles = "admin, seller")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            await _categoryService.DeleteCategoryAsync(id);
            return NoContent();
        }
    }
}
