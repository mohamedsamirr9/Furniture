using System.Text.Json.Serialization;

namespace Furniture.shared.Dtos.SearchWithImage;

public class PythonSearchResult
{
    [JsonPropertyName("product_id")]
    public string ProductId { get; set; } = null!;
    
    [JsonPropertyName("similarity")]
    public float Similarity { get; set; }
}
