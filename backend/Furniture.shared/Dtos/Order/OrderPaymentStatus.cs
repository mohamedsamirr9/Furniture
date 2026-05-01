using System.Text.Json.Serialization;

namespace Furniture.shared.Dtos.Order;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OrderPaymentStatus
{
    Unpaid = 0,
    Paid = 1,
    Failed = 2
}

