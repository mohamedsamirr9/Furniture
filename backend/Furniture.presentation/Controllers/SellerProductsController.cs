using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos.ProductDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Furniture.presentation.Controllers
{
    [Route("api/seller/products")]
    [ApiController]
    [Authorize(Roles = "seller")]
    public class SellerProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public SellerProductsController(IProductService productService)
        {
            _productService = productService;
        }

        private string GetLanguage() =>
            Request.Headers["Accept-Language"].FirstOrDefault()?.Trim() ?? "en";

        private string GetSellerId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return null;
            return userId;
        }

        // GET: api/seller/products
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ProductQueryParams queryParams)
        {
            var sellerId = GetSellerId();
            if (sellerId is null)
                return Unauthorized("User ID not found in claims");

            var result = await _productService.GetSellerProductsAsync(sellerId, queryParams, GetLanguage());
            return Ok(result);
        }

        // GET: api/seller/products/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var sellerId = GetSellerId();
            if (sellerId is null)
                return Unauthorized("User ID not found in claims");

            var result = await _productService.GetByIdAsync(id, GetLanguage());

            if (result is null)
                return NotFound($"Product with id {id} not found");

            return Ok(result);
        }
    }
}