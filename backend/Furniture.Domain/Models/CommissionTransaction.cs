namespace Furniture.Domain.Models;

public class CommissionTransaction
{
    public int Id { get; set; }

    public int SellerProfileId { get; set; }

    public int? OrderId { get; set; }           
    public decimal? OrderTotal { get; set; }    

    public decimal CommissionAmount { get; set; }

    public string Type { get; set; } = null!;
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public SellerProfile SellerProfile { get; set; } = null!;
    public Order? Order { get; set; }           
}
