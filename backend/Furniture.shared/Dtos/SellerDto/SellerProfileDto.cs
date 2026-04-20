namespace Furniture.shared.Dtos.SellerDto
{
    /// <summary>Shared contract for public seller page and authenticated seller dashboard.</summary>
    public class SellerProfileDto
    {
        /// <summary>Identity user id (same as <see cref="SellerId"/>).</summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>Seller identity id (alias of <see cref="Id"/>).</summary>
        public string SellerId { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        /// <summary>Set when the caller is the authenticated seller (dashboard); omitted for anonymous public.</summary>
        public string? Email { get; set; }

        public string Location { get; set; } = string.Empty;
        public string JoinDate { get; set; } = string.Empty;
        public decimal Rating { get; set; }
        public int ReviewsCount { get; set; }
        public int CompletedOrders { get; set; }
        public string Bio { get; set; } = string.Empty;

        /// <summary>Profile image URL (same as <see cref="ProfileImageUrl"/>; kept for backward compatibility).</summary>
        public string AvatarUrl { get; set; } = string.Empty;

        public string ProfileImageUrl { get; set; } = string.Empty;

        public List<string> Specialties { get; set; } = [];
        public List<SellerPortfolioItemDto> Portfolio { get; set; } = [];
    }
}
