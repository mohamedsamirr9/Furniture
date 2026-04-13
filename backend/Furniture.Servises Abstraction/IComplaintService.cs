using Furniture.shared.Dtos.ComplaintsDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Servises_Abstraction
{
    public interface IComplaintService
    {
        Task<ComplaintDto>CreateAsync(string userId, ComplaintCreateDto dto);
        Task<IEnumerable<ComplaintDto>> GetMyAsync(string userId); 
        Task<ComplaintDetailDto> GetByIdAsync(int id);
        Task UpdateAsync(int id, string userId, ComplaintCreateDto dto);
        Task CloseAsync(int id, string userId);
        Task<IEnumerable<ComplaintDto>> GetAllAsync(string? status);

    }
}
