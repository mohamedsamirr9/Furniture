using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos.ProductDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.presentation.Controllers
{
    [ApiController]
    public class ProductImagesController : ControllerBase
    {
        private readonly IProductImageService _productImageService;

        public ProductImagesController(IProductImageService productImageService)
        {
            _productImageService = productImageService;
        }

        // POST /api/products/{id}/images
        [Authorize(Roles ="seller")]
        [HttpPost("api/products/{id}/images")]
        public async Task<IActionResult> AddImage(int id, [FromBody] ProductImageCreateDto dto)
        {
            await _productImageService.AddImageAsync(id, dto.ImageUrl);

            return Ok("Image added successfully");
        }

        // DELETE /api/images/{id}
        [Authorize(Roles ="admin, seller")]
        [HttpDelete("api/images/{id}")]
        public async Task<IActionResult> DeleteImage(int id)
        {
            await _productImageService.DeleteImageAsync(id);

            return NoContent();
        }


    }
}
