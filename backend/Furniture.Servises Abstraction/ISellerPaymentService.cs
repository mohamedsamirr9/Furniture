using Furniture.shared.Dtos.Seller;

namespace Furniture.Servises_Abstraction;

public interface ISellerPaymentService
{
    
    Task<SellerProfileDTO> CreateSellerProfileAsync(string userId, CreateSellerProfileDTO dto);

    
    Task<SellerProfileDTO?> GetMyProfileAsync(string userId);

    
    Task<bool> VerifySellerAsync(int sellerId);

   
    Task<SellerEarningsDTO> GetEarningsAsync(string userId);

   
    Task<List<SellerProfileDTO>> GetAllSellersAsync();

    
    Task<List<SellerProfileDTO>> GetPendingSellersAsync();
}