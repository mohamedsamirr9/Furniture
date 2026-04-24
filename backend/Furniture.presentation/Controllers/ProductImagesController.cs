using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos.ProductDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Furniture.presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductImagesController : ControllerBase
    {
        private readonly IProductImageService _productImageService;
        private readonly IImageValidationService _imageValidationService;

        public ProductImagesController(IProductImageService productImageService,  IImageValidationService imageValidationService)
        {
            _productImageService = productImageService;
            _imageValidationService = imageValidationService;
        }

        // // POST /api/products/{id}/images
        // [Authorize(Roles ="seller")]
        // [HttpPost("api/products/{id}/images")]
        // public async Task<IActionResult> AddImage(int id, [FromBody] ProductImageCreateDto dto)
        // {
        //     await _productImageService.AddImageAsync(id, dto.ImageUrl);
        //
        //     return Ok("Image added successfully");
        // }
        
        
        [Authorize(Roles = "seller")]
        [HttpPost("{id}/images")]  
        public async Task<IActionResult> AddImage(int id, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No image file provided");

            try
            {
                
                using var stream = file.OpenReadStream();
                await _productImageService.AddImageAsync(
                    id, stream, file.FileName, file.ContentType);

                return Ok(new { message = "Image validated and added successfully" });
            }
            catch (Exception ex) when (ex.Message.Contains("rejected"))
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex) when (ex.Message.Contains("not found"))
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(503, new { message = ex.Message });
            }
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
