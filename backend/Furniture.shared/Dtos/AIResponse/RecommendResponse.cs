using System.Text.Json.Serialization;

namespace Furniture.shared.Dtos.AIResponse;

public record RecommendResponse(
    [property: JsonPropertyName("recommendations")] List<RecommendItem> Recommendations);
