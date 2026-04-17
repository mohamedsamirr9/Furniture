using AutoMapper;
using Furniture.Domain.InterfacesRepositories;
using Furniture.Domain.Models;
using Furniture.Domain.Models.Enum;
using Furniture.Services.Specifications;
using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos.ComplaintsDto;

namespace Furniture.Services
{
    public class ComplaintService(IUnitOfWork _unitOfWork, IMapper _mapper) : IComplaintService
    {
        public async Task CloseAsync(int id, string userId, string role)
        {
            var complaint = await GetComplaintDetails(id);

            if (!CanAccessComplaint(complaint, userId, role))
            {
                throw new InvalidOperationException("Unauthorized to access this complaint.");
            }

            if (role.Equals("buyer", StringComparison.OrdinalIgnoreCase) && complaint.UserId != userId)
            {
                throw new InvalidOperationException("Only complaint owner can close it.");
            }

            ValidateTransition(complaint.Status, ComplaintStatus.Closed);
            complaint.Status = ComplaintStatus.Closed;
            _unitOfWork.GetRepository<Complaint, int>().Update(complaint);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<ComplaintDto> CreateAsync(string userId, ComplaintCreateDto dto)
        {
            var orderRepo = _unitOfWork.GetRepository<Order, int>();
            var order = await orderRepo.GetByIdAsync(dto.OrderId);
            if (order == null)
            {
                throw new InvalidOperationException("Order not found.");
            }

            if (order.UserId != userId)
            {
                throw new InvalidOperationException("You can only create complaints for your own orders.");
            }

            var existingComplaints = await _unitOfWork.GetRepository<Complaint, int>()
                .GetAllAsync(new MyComplaintSpecification(userId));
            var hasActiveForOrder = existingComplaints.Any(c => c.OrderId == dto.OrderId && c.Status != ComplaintStatus.Closed);
            if (hasActiveForOrder)
            {
                throw new InvalidOperationException("An active complaint already exists for this order.");
            }

            var complaint = _mapper.Map<Complaint>(dto);
            complaint.UserId = userId;
            await _unitOfWork.GetRepository<Complaint, int>().AddAsync(complaint);
            await _unitOfWork.SaveChangesAsync();

            var created = await GetComplaintDetails(complaint.Id);
            return _mapper.Map<ComplaintDto>(created);
        }

        public async Task<IEnumerable<ComplaintDto>> GetAllAsync(string? status)
        {
            var spec=new ComplaintSpecifications(status);
            var complaints = await _unitOfWork.GetRepository<Complaint, int>().GetAllAsync(spec);
            return _mapper.Map<IEnumerable<ComplaintDto>>(complaints);
        }

        public async Task<ComplaintDetailDto> GetByIdAsync(int id, string userId, string role)
        {
            var complaint = await GetComplaintDetails(id);
            if (!CanAccessComplaint(complaint, userId, role))
            {
                throw new InvalidOperationException("Unauthorized to access this complaint.");
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

        public async Task<ComplaintReplyDto> ReplyAsync(int id, string actorUserId, string actorRole, ReplyComplaintDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Message))
            {
                throw new InvalidOperationException("Reply message is required.");
            }

            if (!actorRole.Equals("seller", StringComparison.OrdinalIgnoreCase) &&
                !actorRole.Equals("admin", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Only seller or admin can reply.");
            }

            var complaint = await GetComplaintDetails(id);
            if (!CanAccessComplaint(complaint, actorUserId, actorRole))
            {
                throw new InvalidOperationException("Unauthorized to access this complaint.");
            }

            var reply = new ComplaintReply
            {
                ComplaintId = complaint.Id,
                ResponderId = actorUserId,
                Message = dto.Message.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.GetRepository<ComplaintReply, int>().AddAsync(reply);
            await _unitOfWork.SaveChangesAsync();

            var updatedComplaint = await GetComplaintDetails(id);
            var latestReply = updatedComplaint.Replies.OrderByDescending(r => r.CreatedAt).First();
            return _mapper.Map<ComplaintReplyDto>(latestReply);
        }

        public async Task UpdateStatusAsync(int id, string actorUserId, string actorRole, UpdateComplaintStatusDto dto)
        {
            if (!Enum.TryParse<ComplaintStatus>(dto.Status, true, out var nextStatus))
            {
                throw new InvalidOperationException("Invalid complaint status.");
            }

            if (!actorRole.Equals("seller", StringComparison.OrdinalIgnoreCase) &&
                !actorRole.Equals("admin", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Only seller or admin can update complaint status.");
            }

            var complaint = await GetComplaintDetails(id);
            if (!CanAccessComplaint(complaint, actorUserId, actorRole))
            {
                throw new InvalidOperationException("Unauthorized to access this complaint.");
            }

            ValidateTransition(complaint.Status, nextStatus);

            complaint.Status = nextStatus;
            _unitOfWork.GetRepository<Complaint, int>().Update(complaint);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateAsync(int id, string userId, ComplaintCreateDto dto)
        {
            var repo = _unitOfWork.GetRepository<Complaint, int>();
            var complaint = await repo.GetByIdAsync(id);
            if (complaint == null)
            {
                throw new InvalidOperationException("Complaint not found.");
            }
            if (complaint.UserId != userId)
            {
                throw new InvalidOperationException("Unauthorized user.");
            }
            if (complaint.Status != ComplaintStatus.Open)
            {
                throw new InvalidOperationException("Complaint can only be updated while status is Open.");
            }
            _mapper.Map(dto, complaint);
            await _unitOfWork.SaveChangesAsync();

        }

        private async Task<Complaint> GetComplaintDetails(int id)
        {
            var spec = new ComplaintWithUserSpecification(id);
            var complaint = await _unitOfWork.GetRepository<Complaint, int>().GetByIdAsync(spec);
            if (complaint == null)
            {
                throw new InvalidOperationException("Complaint not found.");
            }

            return complaint;
        }

        private static bool CanAccessComplaint(Complaint complaint, string actorUserId, string actorRole)
        {
            if (actorRole.Equals("admin", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (actorRole.Equals("buyer", StringComparison.OrdinalIgnoreCase))
            {
                return complaint.UserId == actorUserId;
            }

            if (actorRole.Equals("seller", StringComparison.OrdinalIgnoreCase))
            {
                var orderItemSellerIds = complaint.Order.OrderItems?.Select(oi => oi.SellerId) ?? Enumerable.Empty<string>();
                var offerSellerId = complaint.Order.Offer?.SellerId;
                return orderItemSellerIds.Contains(actorUserId) || offerSellerId == actorUserId;
            }

            return false;
        }

        private static void ValidateTransition(ComplaintStatus current, ComplaintStatus next)
        {
            if (current == next)
            {
                return;
            }

            var allowed = current switch
            {
                ComplaintStatus.Open => next == ComplaintStatus.InProgress,
                ComplaintStatus.InProgress => next == ComplaintStatus.Resolved,
                ComplaintStatus.Resolved => next == ComplaintStatus.Closed,
                ComplaintStatus.Closed => false,
                _ => false
            };

            if (!allowed)
            {
                throw new InvalidOperationException($"Invalid complaint status transition from {current} to {next}.");
            }
        }
    }
}
