using Furniture.shared.Dtos.SellerDto;

namespace Furniture.Servises_Abstraction
{
    public interface ISellerService
    {
        Task<SellerProfileDto?> GetSellerProfileByIdAsync(string sellerId, string language = "en");

        Task<SellerProfileDto?> GetSellerProfileForCurrentUserAsync(string userId, string language = "en");

        Task<bool> UpdateSellerProfileAsync(string userId, UpdateSellerProfileDto dto);
    }
}
