namespace Furniture.shared.Dtos.ProductDtos;

public class ImageValidationSummary
{
    public bool AllApproved { get; set; }
    public List<ImageUrlValidationResult> Results { get; set; } = new();
}