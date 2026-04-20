using System.Text.Json.Serialization;

namespace Furniture.shared.Dtos.AIResponse;

public record RecommendItem(
    [property: JsonPropertyName("product_id")] int ProductId,
    [property: JsonPropertyName("score")]      float Score);
