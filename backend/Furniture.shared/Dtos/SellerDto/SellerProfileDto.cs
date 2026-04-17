namespace Furniture.shared.Dtos.SellerDto
{
    public class SellerProfileDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string JoinDate { get; set; } = string.Empty;
        public decimal Rating { get; set; }
        public int ReviewsCount { get; set; }
        public int CompletedOrders { get; set; }
        public string Bio { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public List<string> Specialties { get; set; } = [];
        public List<SellerPortfolioItemDto> Portfolio { get; set; } = [];
    }
}
