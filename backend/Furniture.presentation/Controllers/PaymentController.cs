using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos.Payment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Furniture.presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

       
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequestDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var result = await _paymentService.CreatePaymentAsync(dto.OrderId, userId);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        
        [HttpPost("callback")]
        [AllowAnonymous]
        public async Task<IActionResult> PaymobCallback([FromBody] PaymobCallbackDTO callback)
        {
            var success = await _paymentService.HandlePaymentCallbackAsync(callback);

            if (success)
                return Ok(new { message = "Payment processed successfully" });

            return BadRequest(new { message = "Payment processing failed" });
        }

        
        [HttpGet("verify/{orderId:int}")]
        [Authorize]
        public async Task<IActionResult> VerifyPayment(int orderId)
        {
            var isPaid = await _paymentService.VerifyPaymentAsync(orderId);
            return Ok(new { isPaid });
        }
    }
}