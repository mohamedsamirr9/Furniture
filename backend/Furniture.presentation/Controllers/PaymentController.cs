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
                 var result = await _paymentService.CreatePaymentAsync(dto.OrderId, userId, dto.PaymentMethod);                 return Ok(result);
             }
             catch (InvalidOperationException ex) when (ex.Message.Contains("profile not found"))
             {
                 return BadRequest(new { message = "Seller profile not found. The seller needs to complete their onboarding first." });
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

            _logger.LogInformation(
                "Paymob callback received: OrderId={OrderId}, MerchantOrderId={MerchantOrderId}, Success={Success}, Id={TransactionId}",
                callback.order, callback.merchant_order_id, callback.success, callback.id); 
            if (!callback.success)
            {
                _logger.LogInformation("Paymob callback: payment not successful, acknowledging");
                return Ok(new { message = "Callback acknowledged (payment not successful)" });
            }

            if (callback.order <= 0)
                _logger.LogWarning("Paymob callback: missing or invalid order id");

            if (string.IsNullOrWhiteSpace(callback.id))
                _logger.LogWarning("Paymob callback: missing transaction id");

            if (string.IsNullOrWhiteSpace(callback.merchant_order_id))
                _logger.LogWarning("Paymob callback: missing merchant_order_id");

            if (callback.order <= 0 && string.IsNullOrWhiteSpace(callback.merchant_order_id))  
            {
                _logger.LogWarning("Paymob callback: no order id or merchant order id, cannot process");
                return Ok(new { message = "Callback acknowledged (insufficient data)" });
            }

            try
            {
                _logger.LogDebug("Processing callback with HMAC present: {HasHmac}", !string.IsNullOrEmpty(hmac));
                var success = await _paymentService.HandlePaymentCallbackAsync(callback, hmac ?? string.Empty);

                _logger.LogInformation(
                    "Paymob callback processed: Success={Success}", success);

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
[HttpPost("post-webhook")] 
[AllowAnonymous]
public async Task<IActionResult> PaymobWebhook([FromQuery] string hmac)
{
    var callback = MapFromWebhookQuery();

    _logger.LogInformation(
        "Paymob webhook received: OrderId={OrderId}, MerchantOrderId={MerchantOrderId}, Success={Success}, Id={TransactionId}",
        callback.order, callback.merchant_order_id, callback.success, callback.id);

    var parts = callback.merchant_order_id?.Split('-');
    string internalOrderId = (parts != null && parts.Length > 1) ? parts[1] : callback.order.ToString();

    if (!callback.success)
    {
        _logger.LogInformation("Paymob webhook: payment not successful");

        if (HttpContext.Request.Method == "GET")
        {

            return Redirect($"https://furniture-mauve-iota.vercel.app/orders/pay?orderId={internalOrderId}&status=failed");
        }

        return Ok(new { message = "Webhook acknowledged (payment not successful)" });
    }

    if (callback.order <= 0 && string.IsNullOrWhiteSpace(callback.merchant_order_id))
    {
        _logger.LogWarning("Paymob webhook: no order id provided");
        return Ok(new { message = "Webhook acknowledged (insufficient data)" });
    }

    try
    {
        _logger.LogDebug("Processing webhook with HMAC present: {HasHmac}", !string.IsNullOrEmpty(hmac));
        var success = await _paymentService.HandlePaymentCallbackAsync(callback, hmac ?? string.Empty);

        _logger.LogInformation("Paymob webhook processed: Success={Success}", success);

        if (HttpContext.Request.Method == "GET")
        {
            return Redirect($"https://furniture-mauve-iota.vercel.app/orders/{internalOrderId}?status=success");
        }

        return success
            ? Ok(new { message = "Webhook processed successfully" })
            : Ok(new { message = "Webhook acknowledged but already processed" });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Paymob webhook: error processing callback");
        
        if (HttpContext.Request.Method == "GET")
        {
             return Redirect($"https://furniture-mauve-iota.vercel.app/orders/pay?orderId={internalOrderId}&status=error");
        }
        
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
            success              = bool.TryParse(Request.Query["success"], out var s) && s,
            id                   = Request.Query["id"].ToString(),
            order                = int.TryParse(Request.Query["order"], out var o) ? o : 0,
            merchant_order_id    = Request.Query["merchant_order_id"].ToString(),
            amount_cents         = Request.Query["amount_cents"].ToString(),
            created_at           = Request.Query["created_at"].ToString(),
            currency             = Request.Query["currency"].ToString(),
            error_occured        = Request.Query["error_occured"].ToString(),
            has_parent_transaction = Request.Query["has_parent_transaction"].ToString(),
            integration_id       = Request.Query["integration_id"].ToString(),
            is_captured          = Request.Query["is_captured"].ToString(),
            is_standalone_payment = Request.Query["is_standalone_payment"].ToString(),
            is_voided            = Request.Query["is_voided"].ToString(),
            owner                = Request.Query["owner"].ToString(),
            pending              = Request.Query["pending"].ToString(),
            source_data_pan      = Request.Query["source_data.pan"].ToString(),
            source_data_sub_type = Request.Query["source_data.sub_type"].ToString(),
            source_data_type     = Request.Query["source_data.type"].ToString()
        };
        
        private PaymobCallbackDTO MapFromWebhookQuery() => new()
        {
          success = bool.TryParse(Request.Query["success"], out var s) && s,
            id = Request.Query["id"].ToString(),
            order = int.TryParse(Request.Query["order"], out var oId) ? oId : 0,
            merchant_order_id = Request.Query["merchant_order_id"].ToString(),
            
            amount_cents = Request.Query["amount_cents"].ToString(),
            created_at = Request.Query["created_at"].ToString(),
            currency = Request.Query["currency"].ToString(),
            error_occured = Request.Query["error_occured"].ToString(),
            has_parent_transaction = Request.Query["has_parent_transaction"].ToString(),
            integration_id = Request.Query["integration_id"].ToString(),
            is_captured = Request.Query["is_captured"].ToString(),
            is_standalone_payment = Request.Query["is_standalone_payment"].ToString(),
            is_voided = Request.Query["is_voided"].ToString(),
            owner = Request.Query["owner"].ToString(),
            pending = Request.Query["pending"].ToString(),
            
            source_data_pan = Request.Query["source_data.pan"].ToString(),
            source_data_sub_type = Request.Query["source_data.sub_type"].ToString(),
            source_data_type = Request.Query["source_data.type"].ToString()
        };
        
        
        #endregion
        
    }
}
