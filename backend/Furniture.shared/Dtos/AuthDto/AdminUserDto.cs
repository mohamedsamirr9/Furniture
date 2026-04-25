namespace Furniture.shared.Dtos.AuthDto
{
    public class AdminUserDto
    {
        public string Id          { get; set; } = null!;
        public string Name        { get; set; } = null!;
        public string Email       { get; set; } = null!;
        public string? Phone      { get; set; }
        public string? Address    { get; set; }
        public string Role        { get; set; } = null!;
        public DateTime JoinDate  { get; set; }
        public string AccountStatus { get; set; } = null!;
    }
}
