using Furniture.Domain.Models.Enum;

namespace Furniture.Domain.Models;

public class SellerRequest
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public string StoreName { get; set; } = null!;

    public string? NationalIdImageUrl { get; set; }

    public SellerRequestStatus Status { get; set; } = SellerRequestStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ReviewedAt { get; set; }

    /// <summary>Admin user id who approved or rejected.</summary>
    public string? ReviewedById { get; set; }

    public string? RejectionReason { get; set; }

    public ApplicationUser User { get; set; } = null!;

    public ApplicationUser? ReviewedBy { get; set; }
}
