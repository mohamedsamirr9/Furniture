namespace Furniture.shared.Dtos.Payment;

public class PaymentResponseDTO
{
    public string PaymentUrl { get; set; } = null!;
    public string PaymentToken { get; set; } = null!;
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Message { get; set; } = "Redirecting to payment gateway";
}