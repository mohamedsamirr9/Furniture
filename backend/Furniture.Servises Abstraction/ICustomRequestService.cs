using Furniture.shared.Dtos.CustomRequestDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    
namespace Furniture.Servises_Abstraction
{
    public interface ICustomRequestService
    {
        Task<CustomRequestDto> CreateAsync(string buyerId, CustomRequestCreateDto dto);
        Task<IEnumerable<CustomRequestDto>> GetMyRequestsAsync(string buyerId);
        Task<CustomRequestDetailsDto> GetByIdAsync(int id);
        Task UpdateAsync(int id, string buyerId, CustomRequestCreateDto dto);
        Task CancelRequest(int id, string buyerId);
        Task<IEnumerable<CustomRequestDto>> GetAllAsync(int pageIndex,  int pageSize, string? status, decimal? minBudget);
    }
}
