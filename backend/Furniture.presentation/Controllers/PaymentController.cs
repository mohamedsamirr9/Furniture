using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos.Payment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace Furniture.presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(IPaymentService paymentService, ILogger<PaymentsController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
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
            var callback = MapFromQuery();

            if (!callback.Success)
            {
                _logger.LogInformation("Paymob callback: payment not successful, acknowledging");
                return Ok(new { message = "Callback acknowledged (payment not successful)" });
            }

            if (callback.OrderId <= 0)
                _logger.LogWarning("Paymob callback: missing or invalid order id");

            if (string.IsNullOrWhiteSpace(callback.TransactionId))
                _logger.LogWarning("Paymob callback: missing transaction id");

            if (string.IsNullOrWhiteSpace(callback.MerchantOrderId))
                _logger.LogWarning("Paymob callback: missing merchant_order_id");

            if (callback.OrderId <= 0 && string.IsNullOrWhiteSpace(callback.MerchantOrderId))
            {
                _logger.LogWarning("Paymob callback: no order id or merchant order id, cannot process");
                return Ok(new { message = "Callback acknowledged (insufficient data)" });
            }

            try
            {
                var success = await _paymentService.HandlePaymentCallbackAsync(callback, hmac ?? string.Empty);

                return success
                    ? Ok(new { message = "Payment processed successfully" })
                    : Ok(new { message = "Callback acknowledged but payment not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Paymob callback: error processing");
                return Ok(new { message = "Callback acknowledged (processing error)" });
            }
        }

        

        [HttpGet("webhook")]
        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> PaymobWebhook([FromQuery] string hmac)
        {
            var callback = MapFromWebhookQuery();

            if (!callback.Success)
            {
                _logger.LogInformation("Paymob webhook: payment not successful, acknowledging");
                return Ok(new { message = "Webhook acknowledged (payment not successful)" });
            }

            if (callback.OrderId <= 0)
                _logger.LogWarning("Paymob webhook: missing or invalid order id - acknowledging anyway");

            if (string.IsNullOrWhiteSpace(callback.TransactionId))
                _logger.LogWarning("Paymob webhook: missing transaction_id - acknowledging anyway");

            if (string.IsNullOrWhiteSpace(callback.MerchantOrderId))
                _logger.LogWarning("Paymob webhook: missing merchant_order_id - acknowledging anyway");

            if (callback.OrderId <= 0 && string.IsNullOrWhiteSpace(callback.MerchantOrderId))
            {
                _logger.LogWarning("Paymob webhook: no order id or merchant order id provided, cannot process");
                return Ok(new { message = "Webhook acknowledged (insufficient data)" });
            }

            try
            {
                var success = await _paymentService.HandlePaymentCallbackAsync(callback, hmac ?? string.Empty);

                return success
                    ? Ok(new { message = "Webhook processed successfully" })
                    : Ok(new { message = "Webhook acknowledged but payment not found or already processed" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Paymob webhook: error processing callback");
                return Ok(new { message = "Webhook acknowledged (processing error)" });
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
