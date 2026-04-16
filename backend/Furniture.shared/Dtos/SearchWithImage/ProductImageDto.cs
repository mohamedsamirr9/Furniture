using System.Text.Json.Serialization;

namespace Furniture.shared.Dtos.SearchWithImage;

public class ProductImageDto
{
    
    
    [JsonPropertyName("product_id")]
    public int ProductId { get; set; }

    [JsonPropertyName("image_url")]
    public string ImageUrl { get; set; }
}
