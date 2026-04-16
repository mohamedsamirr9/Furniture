using System.Text.Json.Serialization;

namespace Furniture.shared.Dtos.ProductDtos;

public class ImageValidationResultDto
{
    [JsonPropertyName("ai_probability")]
    public double AiProbability { get; set; }
    
    [JsonPropertyName("real_probability")]
    public double RealProbability { get; set; }
    
    [JsonPropertyName("decision")]
    public string Decision { get; set; } = null!;

}

