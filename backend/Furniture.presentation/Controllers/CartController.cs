using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Furniture.shared.Dtos.Cart;

namespace Furniture.presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

       private string GetUserId() => "seller-1"; 


        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var cart = await _cartService.GetCartAsync(GetUserId());
            return Ok(cart);
        }


        [HttpPost("items")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartDto dto)
        {
            try
            {
                var cart = await _cartService.AddToCartAsync(GetUserId(), dto);
                return Ok(cart);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }


        [HttpPut("items/{productId}")]
        public async Task<IActionResult> UpdateCartItem(
            int productId, [FromBody] UpdateCartItemDto dto)
        {
            try
            {
                var cart = await _cartService
                    .UpdateCartItemAsync(GetUserId(), productId, dto);
                return Ok(cart);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }


        [HttpDelete("items/{productId}")]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            try
            {
                var cart = await _cartService
                    .RemoveFromCartAsync(GetUserId(), productId);
                return Ok(cart);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }


        [HttpDelete]
        public async Task<IActionResult> ClearCart()
        {
            await _cartService.ClearCartAsync(GetUserId());
            return Ok(new { message = "Cart Has Been Cleared" });
        }
    }
}