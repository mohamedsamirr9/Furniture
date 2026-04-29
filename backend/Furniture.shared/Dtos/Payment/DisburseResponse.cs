namespace Furniture.shared.Dtos.Payment;

public class DisburseResponse
{
    public string transaction_id { get; set; } = null!;
    public string disbursement_status { get; set; } = null!;
    public string status_code { get; set; } = null!;
    public string status_description { get; set; } = null!;
}