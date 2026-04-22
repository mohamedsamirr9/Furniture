using Microsoft.AspNetCore.Mvc;

namespace Furniture.shared.Dtos.Payment
{
    public class PaymobCallbackDTO
    {
        [FromQuery(Name = "success")]
        public bool Success { get; set; }

        [FromQuery(Name = "id")]
        public string Id { get; set; } = string.Empty;

        [FromQuery(Name = "order")]
        public int OrderId { get; set; }

        [FromQuery(Name = "merchant_order_id")]
        public string MerchantOrderId { get; set; } = string.Empty;

        [FromQuery(Name = "amount_cents")]
        public string AmountCents { get; set; } = string.Empty;

        [FromQuery(Name = "created_at")]
        public string CreatedAt { get; set; } = string.Empty;

        [FromQuery(Name = "currency")]
        public string Currency { get; set; } = string.Empty;

        [FromQuery(Name = "error_occured")]
        public string ErrorOccured { get; set; } = string.Empty;

        [FromQuery(Name = "has_parent_transaction")]
        public string HasParentTransaction { get; set; } = string.Empty;

        [FromQuery(Name = "integration_id")]
        public string IntegrationId { get; set; } = string.Empty;

        [FromQuery(Name = "is_captured")]
        public string IsCaptured { get; set; } = string.Empty;

        [FromQuery(Name = "is_standalone_payment")]
        public string IsStandalonePayment { get; set; } = string.Empty;

        [FromQuery(Name = "is_voided")]
        public string IsVoided { get; set; } = string.Empty;

        [FromQuery(Name = "owner")]
        public string OwnerUsername { get; set; } = string.Empty;

        [FromQuery(Name = "pending")]
        public string PendingStatus { get; set; } = string.Empty;

        [FromQuery(Name = "source_data.pan")] // لاحظ النقطة هنا
        public string SourceDataPan { get; set; } = string.Empty;

        [FromQuery(Name = "source_data.sub_type")]
        public string SourceDataSubType { get; set; } = string.Empty;

        [FromQuery(Name = "source_data.type")]
        public string SourceDataType { get; set; } = string.Empty;
    }
}