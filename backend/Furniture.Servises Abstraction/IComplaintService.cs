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
        Task<IEnumerable<ComplaintDto>> GetSellerComplaintsAsync(string sellerId);
        Task<ComplaintDetailDto> GetByIdAsync(int id, string userId, string role);
        Task UpdateAsync(int id, string userId, ComplaintCreateDto dto);
        Task UpdateStatusAsync(int id, string actorUserId, string actorRole, UpdateComplaintStatusDto dto);
        Task<ComplaintReplyDto> ReplyAsync(int id, string actorUserId, string actorRole, ReplyComplaintDto dto);
        Task CloseAsync(int id, string userId, string role);
        Task<IEnumerable<ComplaintDto>> GetAllAsync(string? status);

    }
}
