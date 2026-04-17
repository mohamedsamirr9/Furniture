using Furniture.shared.Dtos.SellerDto;

namespace Furniture.Servises_Abstraction
{
    public interface ISellerService
    {
        Task<SellerProfileDto?> GetSellerProfileByIdAsync(string sellerId, string language = "en");
    }
}
