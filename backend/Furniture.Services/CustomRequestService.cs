using AutoMapper;
using Furniture.Domain.InterfacesRepositories;
using Furniture.Domain.Models;
using Furniture.Domain.Models.Enum;
using Furniture.Services.Specifications;
using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos.CustomRequestDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Services
{
    
    public class CustomRequestService(IUnitOfWork _unitOfWork, IMapper _mapper, INotificationService _notificationService) : ICustomRequestService
    {
        public async Task CancelRequest(int id, string buyerId)
        {
            var repo = _unitOfWork.GetRepository<CustomRequest, int>();
            var request=await repo.GetByIdAsync(id);

            if (request is null)
                throw new Exception("Request not found");
            if (request.BuyerId != buyerId)
                throw new Exception("UnAuthorized");

            request.Status = CustomRequestStatus.Cancelled;
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<CustomRequestDto> CreateAsync(string buyerId, CustomRequestCreateDto dto)
        {
            var repo = _unitOfWork.GetRepository<CustomRequest, int>();
            var request = _mapper.Map<CustomRequest>(dto);
            request.BuyerId = buyerId;
            await repo.AddAsync(request);
            await _unitOfWork.SaveChangesAsync();

            await _notificationService.NotifyAllSellersAsync(
                title: "New Custom Request",
                message: $"New Custom Request Has Been Added: {dto.Description}",
                customRequestId: request.Id
            );  
            
            return _mapper.Map<CustomRequestDto>(request);
        }

        public async Task<IEnumerable<CustomRequestDto>> GetAllAsync(int pageIndex, int pageSize, string? status, decimal? minBudget)
        {
            var spec= new CustomRequestSpecifications(pageIndex, pageSize, status, minBudget);
            var repo=_unitOfWork.GetRepository<CustomRequest, int>();
            var requests= await repo.GetAllAsync(spec);

            return _mapper.Map<IEnumerable<CustomRequestDto>>(requests);
        }

        public async Task<CustomRequestDetailsDto> GetByIdAsync(int id, string userId, string role)
        {
            var spec= new CustomRequestWithOffersSpecifications(id);
            var repo = _unitOfWork.GetRepository<CustomRequest, int>();
            var request= await repo.GetByIdAsync(spec);

            if (request is null)
                throw new Exception("Request not found");
            if (role == "buyer" && request.BuyerId != userId)
                throw new Exception("unauthorized");
            return _mapper.Map<CustomRequestDetailsDto>(request);
        }

        public async Task<IEnumerable<CustomRequestDto>> GetMyRequestsAsync(string buyerId)
        {
            var spec = new MyCustomRequestsSpecifications(buyerId);
            var repo = _unitOfWork.GetRepository<CustomRequest, int>();
            var requests =await repo.GetAllAsync(spec);

            return _mapper.Map<IEnumerable<CustomRequestDto>>(requests);
        }

        public async Task UpdateAsync(int id, string buyerId, CustomRequestCreateDto dto)
        {
            var repo = _unitOfWork.GetRepository<CustomRequest, int>();
            var request=await repo.GetByIdAsync(id);

            if (request is null)
                throw new Exception("Request not found");

            if (request.BuyerId != buyerId)
                throw new Exception("UnAuthorized");

            if (request.Status != CustomRequestStatus.Open)
                throw new Exception("Only open requested can be updated");

            _mapper.Map(dto, request);
            repo.Update(request);
            await _unitOfWork.SaveChangesAsync();
            
            await _notificationService.NotifyAllSellersAsync(
                title: "Custom Request Updated",
                message: $"A custom request has been updated: {dto.Description}",
                customRequestId: id
            );
        }
    }
}
