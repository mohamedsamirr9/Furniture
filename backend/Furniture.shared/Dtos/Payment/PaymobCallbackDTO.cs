namespace Furniture.shared.Dtos.Payment
{
    public class PaymobCallbackDTO
    {
        public bool success { get; set; }
        public string id { get; set; } = string.Empty;
        public int order { get; set; } 
        public string merchant_order_id { get; set; } = string.Empty;
        public string amount_cents { get; set; } = string.Empty;
        public string created_at { get; set; } = string.Empty;
        public string currency { get; set; } = string.Empty;
        public string error_occured { get; set; } = string.Empty;
        public string has_parent_transaction { get; set; } = string.Empty;
        public string integration_id { get; set; } = string.Empty;
        public string is_captured { get; set; } = string.Empty;
        public string is_standalone_payment { get; set; } = string.Empty;
        public string is_voided { get; set; } = string.Empty;
        public string owner { get; set; } = string.Empty;
        public string pending { get; set; } = string.Empty;
        
        public string source_data_pan { get; set; } = string.Empty;
        public string source_data_sub_type { get; set; } = string.Empty;
        public string source_data_type { get; set; } = string.Empty;
    }
}