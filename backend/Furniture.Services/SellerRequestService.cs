using Furniture.Domain.InterfacesRepositories;
using Furniture.Domain.Models;
using Furniture.Domain.Models.Enum;
using Furniture.Services.Specifications;
using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos.SellerRequest;
using Microsoft.AspNetCore.Identity;

namespace Furniture.Services;

public class SellerRequestService(
    IUnitOfWork unitOfWork,
    UserManager<ApplicationUser> userManager) : ISellerRequestService
{
    public async Task<IReadOnlyList<SellerRequestDto>> GetSellerRequestsForAdminAsync(SellerRequestStatus status)
    {
        var repo = unitOfWork.GetRepository<SellerRequest, int>();
        var list = await repo.GetAllAsync(new SellerRequestsByStatusSpecification(status));
        return list.Select(Map).ToList();
    }

    public async Task<SellerRequestDto?> GetByIdAsync(int id)
    {
        var repo = unitOfWork.GetRepository<SellerRequest, int>();
        var entity = await repo.GetByIdAsync(new SellerRequestByIdSpecification(id));
        return entity is null ? null : Map(entity);
    }

    public async Task ApproveAsync(int requestId, string adminUserId)
    {
        var repo = unitOfWork.GetRepository<SellerRequest, int>();
        var request = await repo.GetByIdAsync(new SellerRequestByIdSpecification(requestId));
        if (request is null)
            throw new Exception("Seller request not found");

        if (request.Status != SellerRequestStatus.Pending)
            throw new Exception("Request is not pending");

        var user = await userManager.FindByIdAsync(request.UserId);
        if (user is null)
            throw new Exception("User not found");

        if (user.Role == Roles.seller)
            throw new Exception("User is already a seller");

        user.Role = Roles.seller;
        user.IsVerified = false;
        if (!string.IsNullOrEmpty(request.NationalIdImageUrl))
            user.NationalIdImage = request.NationalIdImageUrl;

        var identityResult = await userManager.UpdateAsync(user);
        if (!identityResult.Succeeded)
            throw new Exception(string.Join(", ", identityResult.Errors.Select(e => e.Description)));

        var profileRepo = unitOfWork.GetRepository<SellerProfile, int>();
        var existingProfile = await profileRepo.GetByIdAsync(new SellerProfileByUserIdSpecification(request.UserId));
        if (existingProfile is null)
        {
            var sellerProfile = new SellerProfile
            {
                UserId = request.UserId,
                StoreName = request.StoreName,
                StoreDescription = null,
                CommissionRate = 6m,
                IsVerified = false,
                CreatedAt = DateTime.UtcNow
            };
            await profileRepo.AddAsync(sellerProfile);
        }

        request.Status = SellerRequestStatus.Approved;
        request.ReviewedAt = DateTime.UtcNow;
        request.ReviewedById = adminUserId;

        repo.Update(request);
        await unitOfWork.SaveChangesAsync();
    }

    public async Task RejectAsync(int requestId, string adminUserId, RejectSellerRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Reason))
            throw new Exception("Rejection reason is required");

        var repo = unitOfWork.GetRepository<SellerRequest, int>();
        var request = await repo.GetByIdAsync(requestId);
        if (request is null)
            throw new Exception("Seller request not found");

        if (request.Status != SellerRequestStatus.Pending)
            throw new Exception("Request is not pending");

        request.Status = SellerRequestStatus.Rejected;
        request.RejectionReason = dto.Reason.Trim();
        request.ReviewedAt = DateTime.UtcNow;
        request.ReviewedById = adminUserId;

        repo.Update(request);
        await unitOfWork.SaveChangesAsync();
    }

    public async Task<SellerRequestDto?> GetMyRequestAsync(string userId)
    {
        var repo = unitOfWork.GetRepository<SellerRequest, int>();

        var pending = await repo.GetByIdAsync(new PendingSellerRequestForUserSpecification(userId));
        if (pending is not null)
            return Map(pending);

        var latestBatch = await repo.GetAllAsync(new LatestSellerRequestsForUserSpecification(userId));
        var latest = latestBatch.FirstOrDefault();
        return latest is null ? null : Map(latest);
    }

    private static SellerRequestDto Map(SellerRequest r)
    {
        return new SellerRequestDto
        {
            Id = r.Id,
            UserId = r.UserId,
            UserEmail = r.User?.Email,
            UserName = r.User?.Name,
            StoreName = r.StoreName,
            NationalIdImageUrl = r.NationalIdImageUrl,
            Status = r.Status.ToString(),
            CreatedAt = r.CreatedAt,
            ReviewedAt = r.ReviewedAt,
            ReviewedById = r.ReviewedById,
            RejectionReason = r.RejectionReason
        };
    }
}
