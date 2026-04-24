using Furniture.shared.Dtos.Payment;

namespace Furniture.Servises_Abstraction
{
    public interface IPaymentService
    {
        Task<PaymentResponseDTO> CreatePaymentAsync(int orderId, string userId);

        Task<bool> HandlePaymentCallbackAsync(PaymobCallbackDTO callback, string hmac);

        Task<bool> VerifyPaymentAsync(int orderId);
    }
}