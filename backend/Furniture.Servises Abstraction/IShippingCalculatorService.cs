using System.Collections.Generic;
using System.Threading.Tasks;

namespace Furniture.Servises_Abstraction
{
    public interface IShippingCalculatorService
    {
        Task<(decimal ShippingCost, int? ShippingRuleId)> CalculateShippingAsync(string city, IEnumerable<int> categoryIds);
        Task<decimal> GetShippingEstimateAsync(string userId, string city, int? offerId);
    }
}
