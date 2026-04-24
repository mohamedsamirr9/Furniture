namespace Furniture.Domain.Models;

public class UserPreference
{
    public int Id { get; set; }
    public string UserId { get; set; } = null!;
    public string? EmbeddingVector { get; set; }
    public string? Style { get; set; }
    public string? Color { get; set; }
    public string? RoomSize { get; set; }
    public string? Budget { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ApplicationUser User { get; set; } = null!;
}