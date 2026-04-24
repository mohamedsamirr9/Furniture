namespace Furniture.shared.Dtos.Payment;

public class PayoutsTokenResponse
{
    public string access_token { get; set; } = null!;
    public string refresh_token { get; set; } = null!;
    public int expires_in { get; set; }
}
