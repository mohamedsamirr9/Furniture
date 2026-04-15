using AutoMapper;
using Furniture.Domain.InterfacesRepositories;
using Furniture.Domain.Models;
using Furniture.Domain.Models.Enum;
using Furniture.Services.Specifications;
using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos.ComplaintsDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Services
{
    public class ComplaintService(IUnitOfWork _unitOfWork, IMapper _mapper) : IComplaintService
    {
        public async Task CloseAsync(int id, string userId)
        {
            var repo = _unitOfWork.GetRepository<Complaint, int>();
            var complaint = await repo.GetByIdAsync(id);
            if (complaint == null) 
            {
                throw new Exception
                    (
                    "complaint not found!"
                    );
            }
            if (complaint.UserId != userId) 
            {
                throw new Exception
                    (
                    "Unauthorised user!"
                    );
            }
            complaint.Status = ComplaintStatus.Resolved;
            repo.Update(complaint);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<ComplaintDto> CreateAsync(string userId, ComplaintCreateDto dto)
        {
            var repo = _unitOfWork.GetRepository<Complaint, int>();
            var complaint = _mapper.Map<Complaint>(dto);
            complaint.UserId = userId;
            await repo.AddAsync(complaint);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<ComplaintDto>(complaint);
        }

        public async Task<IEnumerable<ComplaintDto>> GetAllAsync(string? status)
        {
            var spec=new ComplaintSpecifications(status);
            var complaints = await _unitOfWork.GetRepository<Complaint, int>().GetAllAsync(spec);
            return _mapper.Map<IEnumerable<ComplaintDto>>(complaints);
        }

        public async Task<ComplaintDetailDto> GetByIdAsync(int id)
        {
            var spec = new ComplaintWithUserSpecification(id);
            var complaint = await _unitOfWork.GetRepository<Complaint, int>().GetByIdAsync(spec);
            if (complaint == null) 
            {
                throw new Exception
                    (
                    "Complaint Not Fount!"
                    );
            }
            return _mapper.Map<ComplaintDetailDto>(complaint);
        }

        public async Task<IEnumerable<ComplaintDto>> GetMyAsync(string userId)
        {
            var spec = new MyComplaintSpecification(userId);
            var complaints = await _unitOfWork.GetRepository<Complaint, int>().GetAllAsync(spec);
            return _mapper.Map<IEnumerable<ComplaintDto>>(complaints);
        }

        public async Task<IEnumerable<ComplaintDto>> GetSellerComplaintsAsync(string sellerId)
        {
            var spec = new SellerComplaintSpecification(sellerId);
            var complaints = await _unitOfWork.GetRepository<Complaint, int>().GetAllAsync(spec);
            return _mapper.Map<IEnumerable<ComplaintDto>>(complaints);
        }

        public async Task UpdateAsync(int id, string userId, ComplaintCreateDto dto)
        {
            var repo = _unitOfWork.GetRepository<Complaint, int>();
            var complaint = await repo.GetByIdAsync(id);
            if (complaint == null)
            {
                throw new Exception
                    (
                    "Compliant Not Found"
                    );
            }
            if (complaint.UserId != userId)
            {
                throw new Exception
                    (
                    "Unauthorized User!"
                    );
            }
            if (complaint.Status != ComplaintStatus.Open)
            {
                throw new Exception
                    (
                    "Your Complaint is still under review, please wait till resolving before submitting an update!"
                    );
            }
            _mapper.Map(dto, complaint);
            await _unitOfWork.SaveChangesAsync();

        }
    }
}
