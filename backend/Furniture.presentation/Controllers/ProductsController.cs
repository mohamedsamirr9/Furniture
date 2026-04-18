using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos.ProductDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Furniture.presentation.Controllers
{
    /// <summary>Public catalog and seller product management under <c>/api/products</c>.</summary>
    [Route("api/products")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private const int MaxProductImages = 5;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        private string GetLanguage() =>
            Request.Headers["Accept-Language"].FirstOrDefault()?.Trim() ?? "en";

        private string? GetCurrentUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        #region Public

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ProductQueryParams queryParams)
        {
            var result = await _productService.GetAllAsync(queryParams, GetLanguage());
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _productService.GetByIdAsync(id, GetLanguage());

            if (result is null)
                return NotFound($"Product with id {id} not found");

            return Ok(result);
        }

        #endregion

        #region Seller

        [Authorize(Roles = "seller")]
        [HttpGet("seller")]
        public async Task<IActionResult> GetSellerProducts([FromQuery] ProductQueryParams queryParams)
        {
            var sellerId = GetCurrentUserId();
            if (string.IsNullOrEmpty(sellerId))
                return Unauthorized("User ID not found in claims");

            var result = await _productService.GetSellerProductsAsync(sellerId, queryParams, GetLanguage());
            return Ok(result);
        }

        [Authorize(Roles = "seller")]
        [HttpGet("seller/{id:int}")]
        public async Task<IActionResult> GetSellerProductById(int id)
        {
            var sellerId = GetCurrentUserId();
            if (string.IsNullOrEmpty(sellerId))
                return Unauthorized("User ID not found in claims");

            var result = await _productService.GetByIdAsync(id, GetLanguage());

            if (result is null)
                return NotFound($"Product with id {id} not found");

            if (!string.Equals(result.SellerId, sellerId, StringComparison.Ordinal))
                return NotFound($"Product with id {id} not found");

            return Ok(result);
        }

        [Authorize(Roles = "seller,admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductCreateUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.ImageUrls != null && dto.ImageUrls.Count > MaxProductImages)
                return BadRequest($"A product can have at most {MaxProductImages} images.");

            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID not found in claims");

            if (User.IsInRole("admin"))
            {
                if (string.IsNullOrWhiteSpace(dto.SellerId))
                    return BadRequest(new { message = "SellerId is required when creating a product as admin." });
            }
            else
                dto.SellerId = userId;

            var result = await _productService.CreateAsync(dto, GetLanguage());

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Id },
                result
            );
        }

        [Authorize(Roles = "seller,admin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductCreateUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.ImageUrls != null && dto.ImageUrls.Count > MaxProductImages)
                return BadRequest($"A product can have at most {MaxProductImages} images.");

            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID not found in claims");

            if (!User.IsInRole("admin"))
                dto.SellerId = userId;

            await _productService.UpdateAsync(id, dto);

            return NoContent();
        }

        [Authorize(Roles = "seller,admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _productService.DeleteAsync(id);

            return NoContent();
        }

        #endregion
    }
}
