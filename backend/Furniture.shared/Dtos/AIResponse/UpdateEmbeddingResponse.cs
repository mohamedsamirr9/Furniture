using System.Text.Json.Serialization;

namespace Furniture.shared.Dtos.AIResponse;

public record UpdateEmbeddingResponse(
    [property: JsonPropertyName("embedding")] List<float> Embedding);
