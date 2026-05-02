using Furniture.Domain.Models.Enum;
using Furniture.shared.Dtos.SellerRequest;

namespace Furniture.Servises_Abstraction;

public interface ISellerRequestService
{
    Task<IReadOnlyList<SellerRequestDto>> GetSellerRequestsForAdminAsync(SellerRequestStatus status);

    Task<SellerRequestDto?> GetByIdAsync(int id);

    Task ApproveAsync(int requestId, string adminUserId);

    Task RejectAsync(int requestId, string adminUserId, RejectSellerRequestDto dto);

    /// <summary>Most relevant application for the user: pending if any, otherwise latest by date.</summary>
    Task<SellerRequestDto?> GetMyRequestAsync(string userId);
}
