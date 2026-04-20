namespace Furniture.shared.Dtos.ProductDtos;

public class ImageUrlValidationResult
{
    public string Url { get; set; } = null!;
    public string Decision { get; set; } = null!;
    public float AiProbability { get; set; }
}