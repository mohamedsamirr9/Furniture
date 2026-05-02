namespace Furniture.shared.Dtos.SellerRequest;

public class SellerRequestDto
{
    public int Id { get; set; }

    public string UserId { get; set; } = "";

    public string? UserEmail { get; set; }

    public string? UserName { get; set; }

    public string StoreName { get; set; } = "";

    public string? NationalIdImageUrl { get; set; }

    public string Status { get; set; } = "";

    public DateTime CreatedAt { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public string? ReviewedById { get; set; }

    public string? RejectionReason { get; set; }
}
