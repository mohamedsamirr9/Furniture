using System.Text.Json.Serialization;

namespace Furniture.shared.Dtos.Payment
{
    public class PaymobCallbackDTO
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("order")]
        public int OrderId { get; set; }

        [JsonPropertyName("transaction_id")]
        public string TransactionId { get; set; } = string.Empty;

        [JsonPropertyName("amount_cents")]
        public string AmountCents { get; set; } = string.Empty;

        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; } = string.Empty;

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = string.Empty;

        [JsonPropertyName("error_occured")]
        public string ErrorOccured { get; set; } = string.Empty;

        [JsonPropertyName("has_parent_transaction")]
        public string HasParentTransaction { get; set; } = string.Empty;

        [JsonPropertyName("integration_id")]
        public string IntegrationId { get; set; } = string.Empty;

        [JsonPropertyName("is_captured")]
        public string IsCaptured { get; set; } = string.Empty;

        [JsonPropertyName("is_refunded_transaction")]
        public string IsRefundedTransaction { get; set; } = string.Empty;

        [JsonPropertyName("is_standalone_payment")]
        public string IsStandalonePayment { get; set; } = string.Empty;

        [JsonPropertyName("is_voided")]
        public string IsVoided { get; set; } = string.Empty;

        [JsonPropertyName("owner")]
        public string OwnerUsername { get; set; } = string.Empty;

        [JsonPropertyName("pending")]
        public string PendingStatus { get; set; } = string.Empty;

        [JsonPropertyName("source_data_pan")]
        public string SourceDataPan { get; set; } = string.Empty;

        [JsonPropertyName("source_data_sub_type")]
        public string SourceDataSubType { get; set; } = string.Empty;

        [JsonPropertyName("source_data_type")]
        public string SourceDataType { get; set; } = string.Empty;
    }
}