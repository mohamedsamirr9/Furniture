using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos.Payment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

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

        

        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> PaymobWebhook(
            [FromBody] JsonElement payload,
            [FromQuery] string hmac)
        {
            var callback = MapFromWebhookPayload(payload);
            var resolvedHmac = !string.IsNullOrWhiteSpace(hmac)
                ? hmac
                : GetString(payload, "hmac");

            if (string.IsNullOrEmpty(resolvedHmac))
                return Unauthorized(new { message = "HMAC is missing" });

            try
            {
                var success = await _paymentService.HandlePaymentCallbackAsync(callback, resolvedHmac);

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

        private static PaymobCallbackDTO MapFromWebhookPayload(JsonElement payload)
        {
            var source = payload;
            if (payload.TryGetProperty("obj", out var obj))
            {
                source = obj;
            }

            var sourceData = source.TryGetProperty("source_data", out var sd)
                ? sd
                : default;

            var orderElement = source.TryGetProperty("order", out var orderValue)
                ? orderValue
                : default;

            var orderId = GetOrderId(orderElement);
            var merchantOrderId = GetString(source, "merchant_order_id");

            if (string.IsNullOrWhiteSpace(merchantOrderId) &&
                orderElement.ValueKind == JsonValueKind.Object)
            {
                merchantOrderId = GetString(orderElement, "merchant_order_id");
            }

            return new PaymobCallbackDTO
            {
                Success = GetBool(source, "success"),
                Id = GetString(source, "id"),
                OrderId = orderId,
                MerchantOrderId = merchantOrderId,
                TransactionId = GetString(source, "transaction_id"),
                AmountCents = GetString(source, "amount_cents"),
                CreatedAt = GetString(source, "created_at"),
                Currency = GetString(source, "currency"),
                ErrorOccured = GetString(source, "error_occured"),
                HasParentTransaction = GetString(source, "has_parent_transaction"),
                IntegrationId = GetString(source, "integration_id"),
                IsCaptured = GetString(source, "is_captured"),
                IsRefundedTransaction = GetString(source, "is_refunded_transaction"),
                IsStandalonePayment = GetString(source, "is_standalone_payment"),
                IsVoided = GetString(source, "is_voided"),
                OwnerUsername = GetString(source, "owner"),
                PendingStatus = GetString(source, "pending"),
                SourceDataPan = sourceData.ValueKind == JsonValueKind.Object ? GetString(sourceData, "pan") : string.Empty,
                SourceDataSubType = sourceData.ValueKind == JsonValueKind.Object ? GetString(sourceData, "sub_type") : string.Empty,
                SourceDataType = sourceData.ValueKind == JsonValueKind.Object ? GetString(sourceData, "type") : string.Empty
            };
        }

        private static int GetOrderId(JsonElement orderElement)
        {
            if (orderElement.ValueKind == JsonValueKind.Number &&
                orderElement.TryGetInt32(out var numericOrderId))
            {
                return numericOrderId;
            }

            if (orderElement.ValueKind == JsonValueKind.String &&
                int.TryParse(orderElement.GetString(), out var stringOrderId))
            {
                return stringOrderId;
            }

            if (orderElement.ValueKind == JsonValueKind.Object)
            {
                if (orderElement.TryGetProperty("id", out var idProp))
                {
                    if (idProp.ValueKind == JsonValueKind.Number &&
                        idProp.TryGetInt32(out var nestedNumericOrderId))
                    {
                        return nestedNumericOrderId;
                    }

                    if (idProp.ValueKind == JsonValueKind.String &&
                        int.TryParse(idProp.GetString(), out var nestedStringOrderId))
                    {
                        return nestedStringOrderId;
                    }
                }
            }

            return 0;
        }

        private static string GetString(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var propertyValue))
                return string.Empty;

            return propertyValue.ValueKind switch
            {
                JsonValueKind.String => propertyValue.GetString() ?? string.Empty,
                JsonValueKind.Number => propertyValue.GetRawText(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                _ => string.Empty
            };
        }

        private static bool GetBool(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var propertyValue))
                return false;

            return propertyValue.ValueKind switch
            {
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.String => bool.TryParse(propertyValue.GetString(), out var parsed) && parsed,
                _ => false
            };
        }
        
        
        #endregion
        
    }
}