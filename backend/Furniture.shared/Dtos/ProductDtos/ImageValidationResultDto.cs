using System.Text.Json.Serialization;

namespace Furniture.shared.Dtos.ProductDtos;

public class ImageValidationResult
{
    [JsonPropertyName("ai_probability")]
    public float AiProbability { get; set; }

    [JsonPropertyName("real_probability")]
    public float RealProbability { get; set; }

    [JsonPropertyName("decision")]
    public string Decision { get; set; } = null!;

    public bool IsApproved => Decision == "approve";
}
