namespace Furniture.shared.Dtos.Payment;

public class PaymentResponseDTO
{
    public string? PaymentUrl { get; set; }
    public string? PaymentToken { get; set; }
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string? Message { get; set; }
}

