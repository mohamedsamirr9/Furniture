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
        [HttpPost("api/products/{id}/images")]
        public async Task<IActionResult> AddImage(int id, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No image file provided");

            ImageValidationResult validationResult;
            try
            {
                using var stream = file.OpenReadStream();
                validationResult = await _imageValidationService.ValidateAsync(
                    stream, file.FileName, file.ContentType);
            }
            catch (Exception ex)
            {
                return StatusCode(503, $"Image validation service unavailable: {ex.Message}");
            }

            if (!validationResult.IsApproved)
            {
                return BadRequest(new
                {
                    message = "Image rejected: AI-generated content detected",
                    aiProbability = validationResult.AiProbability,
                    decision = validationResult.Decision
                });
            }
            
            return Ok(new
            {
                message = "Image validated and added successfully",
                aiProbability = validationResult.AiProbability
            });
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
