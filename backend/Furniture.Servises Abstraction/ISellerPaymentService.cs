using Furniture.shared.Dtos.Payment;
using Furniture.shared.Dtos.Seller;

namespace Furniture.Servises_Abstraction
{
    public interface ISellerPaymentService
    {
        Task<SellerProfileDTO> CreateSellerProfileAsync(string userId, CreateSellerProfileDTO dto);
        Task<SellerProfileDTO?> GetMyProfileAsync(string userId);
        Task<SellerEarningsDTO> GetEarningsAsync(string userId);
        Task<List<SellerPayoutDTO>> GetSellerPayoutsAsync(string userId);
        Task<List<SellerProfileDTO>> GetAllSellersAsync();
        Task<List<SellerProfileDTO>> GetPendingSellersAsync();
        Task<bool> VerifySellerAsync(int sellerId);
        Task<bool> RetryFailedPayoutAsync(int payoutId);
        Task ProcessPayoutsForOrderAsync(int orderId);
        
        Task<SellerDebtDTO> GetSellerDebtAsync(string userId);
        Task<bool> UnblockSellerAsync(int sellerId);
        Task<bool> SettleSellerDebtAsync(int sellerId, decimal amount);
        Task<SellerExposureDTO> GetSellerExposureAsync(int sellerId);
    }
}