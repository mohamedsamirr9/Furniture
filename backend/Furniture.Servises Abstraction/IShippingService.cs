using Furniture.shared.Dtos.ShippingRule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Servises_Abstraction
{
    public interface IShippingService
    {
        Task<IEnumerable<ShippingRuleDto>> GetAllAsync(string? city, int? categoryId);
        Task<ShippingRuleDto?> GetByIdAsync(int id);
        Task<ShippingRuleDto> CreateAsync(ShippingRuleCreateUpdateDto dto);
        Task UpdateAsync(int id, ShippingRuleCreateUpdateDto dto);
        Task DeleteAsync(int id);
    }
}