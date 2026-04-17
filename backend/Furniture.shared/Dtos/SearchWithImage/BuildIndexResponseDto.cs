using System.Text.Json.Serialization;

namespace Furniture.shared.Dtos.SearchWithImage;

public class BuildIndexResponseDto
{
    [JsonPropertyName("indexed_count")]
    public int IndexedCount { get; set; }
    
    [JsonPropertyName("message")]
    public string Message   { get; set; } = null!;
}


