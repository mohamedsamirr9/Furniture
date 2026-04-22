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

        
        [HttpGet("callback")]
        [AllowAnonymous]
        public async Task<IActionResult> PaymobCallback([FromQuery] string hmac)
        {
            if (string.IsNullOrWhiteSpace(hmac))
                return Unauthorized(new { message = "HMAC is missing" });

            var callback = MapFromQuery();

            try
            {
                var success = await _paymentService.HandlePaymentCallbackAsync(callback, hmac);

                return success
                    ? Ok(new { message = "Payment processed successfully" })
                    : BadRequest(new { message = "Payment processing failed" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        

        [HttpGet("webhook")]
        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> PaymobWebhook([FromQuery] string hmac)
        {
            if (string.IsNullOrWhiteSpace(hmac))
                return Unauthorized(new { message = "HMAC is missing" });

            var callback = MapFromWebhookQuery();

            // Paymob can send unsuccessful attempts; acknowledge without updating state.
            if (!callback.Success)
                return Ok(new { message = "Webhook ignored (payment not successful)" });

            if (callback.OrderId <= 0)
                return BadRequest(new { message = "Invalid or missing order id" });

            if (string.IsNullOrWhiteSpace(callback.TransactionId))
                return BadRequest(new { message = "Invalid or missing transaction id" });

            if (string.IsNullOrWhiteSpace(callback.MerchantOrderId))
                return BadRequest(new { message = "Invalid or missing merchant_order_id" });

            try
            {
                var success = await _paymentService.HandlePaymentCallbackAsync(callback, hmac);

                return success
                    ? Ok(new { message = "Webhook processed successfully" })
                    : BadRequest(new { message = "Webhook processing failed" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        

        [HttpGet("verify/{orderId:int}")]
        [Authorize]
        public async Task<IActionResult> VerifyPayment(int orderId)
        {
            var isPaid = await _paymentService.VerifyPaymentAsync(orderId);
            return Ok(new { isPaid });
        }

       
        #region Private Helpers

        
        private PaymobCallbackDTO MapFromQuery() => new()
        {
            Success          = bool.TryParse(Request.Query["success"], out var s) && s,
            Id               = Request.Query["id"].ToString(),
            OrderId          = int.TryParse(Request.Query["order"], out var o) ? o : 0,
            MerchantOrderId  = Request.Query["merchant_order_id"].ToString(),
            TransactionId    = Request.Query["id"].ToString(),
            AmountCents      = Request.Query["amount_cents"].ToString(),
            CreatedAt        = Request.Query["created_at"].ToString(),
            Currency         = Request.Query["currency"].ToString(),
            ErrorOccured     = Request.Query["error_occured"].ToString(),
            HasParentTransaction  = Request.Query["has_parent_transaction"].ToString(),
            IntegrationId    = Request.Query["integration_id"].ToString(),
            IsCaptured       = Request.Query["is_captured"].ToString(),
            IsRefundedTransaction = Request.Query["is_refunded_transaction"].ToString(),
            IsStandalonePayment   = Request.Query["is_standalone_payment"].ToString(),
            IsVoided         = Request.Query["is_voided"].ToString(),
            OwnerUsername    = Request.Query["owner"].ToString(),
            PendingStatus    = Request.Query["pending"].ToString(),
            SourceDataPan    = Request.Query["source_data.pan"].ToString(),
            SourceDataSubType = Request.Query["source_data.sub_type"].ToString(),
            SourceDataType   = Request.Query["source_data.type"].ToString()
        };
        
        private PaymobCallbackDTO MapFromWebhookQuery() => new()
        {
            Success = bool.TryParse(Request.Query["success"], out var s) && s,
            Id = Request.Query["id"].ToString(),
            OrderId = int.TryParse(Request.Query["order"], out var orderId) ? orderId : 0,
            MerchantOrderId = Request.Query["merchant_order_id"].ToString(),
            TransactionId = !string.IsNullOrWhiteSpace(Request.Query["transaction_id"])
                ? Request.Query["transaction_id"].ToString()
                : Request.Query["id"].ToString(),
            AmountCents = Request.Query["amount_cents"].ToString(),
            CreatedAt = Request.Query["created_at"].ToString(),
            Currency = Request.Query["currency"].ToString(),
            ErrorOccured = Request.Query["error_occured"].ToString(),
            HasParentTransaction = Request.Query["has_parent_transaction"].ToString(),
            IntegrationId = Request.Query["integration_id"].ToString(),
            IsCaptured = Request.Query["is_captured"].ToString(),
            IsRefundedTransaction = Request.Query["is_refunded_transaction"].ToString(),
            IsStandalonePayment = Request.Query["is_standalone_payment"].ToString(),
            IsVoided = Request.Query["is_voided"].ToString(),
            OwnerUsername = Request.Query["owner"].ToString(),
            PendingStatus = Request.Query["pending"].ToString(),
            SourceDataPan = Request.Query["source_data.pan"].ToString(),
            SourceDataSubType = Request.Query["source_data.sub_type"].ToString(),
            SourceDataType = Request.Query["source_data.type"].ToString()
        };
        
        
        #endregion
        
    }
}
