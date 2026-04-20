using System.Text.Json.Serialization;

namespace Furniture.shared.Dtos.AIResponse;

public record EmbedProductResponse(
    [property: JsonPropertyName("product_id")] int ProductId,
    [property: JsonPropertyName("embedding")]  List<float> Embedding);
