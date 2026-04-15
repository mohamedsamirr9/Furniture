namespace Furniture.shared.Dtos.Payment;

public class PaymobCallbackDTO
{
    public bool Success { get; set; }
    public int OrderId { get; set; }
    public string TransactionId { get; set; } = null!;
    public int AmountCents { get; set; }
}