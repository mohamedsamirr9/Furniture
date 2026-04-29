using System.Text.Json.Serialization;

namespace Furniture.shared.Dtos.SearchWithImage;

public class BuildIndexRequestDto
{
    [JsonPropertyName("products")]
    public List<ProductImageDto> Products { get; set; }}
