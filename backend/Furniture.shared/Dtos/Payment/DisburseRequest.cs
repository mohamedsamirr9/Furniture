namespace Furniture.shared.Dtos.Payment;

public class DisburseRequest
{
    public string issuer { get; set; } = null!;        
    public decimal amount { get; set; }
    public string full_name { get; set; } = null!;
    public string bank_card_number { get; set; } = null!;
    public string bank_code { get; set; } = null!;
    public string bank_transaction_type { get; set; } = "cash_transfer";
    public string? client_reference { get; set; }       
}