namespace Furniture.shared.Dtos.SearchWithImage;

public class ProductSearchResultDto
{
    public int ProductId    { get; set; }
    public string Name      { get; set; } = null!;
    public decimal Price    { get; set; }
    public float Similarity { get; set; }
    public string? ImageUrl { get; set; }
}