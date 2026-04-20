using System.Text.Json.Serialization;

namespace Furniture.shared.Dtos.AIResponse;

public record EmbedQuizResponse(
    [property: JsonPropertyName("embedding")] List<float> Embedding);
