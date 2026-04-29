namespace Furniture.shared.Dtos.Payment;

public class CreatePaymentRequestDTO
{
    public int OrderId { get; set; }
    public string PaymentMethod { get; set; } = "card";

}