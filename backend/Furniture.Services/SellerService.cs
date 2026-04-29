using Furniture.Domain.InterfacesRepositories;
using Furniture.Domain.Models;
using Furniture.Domain.Models.Enum;
using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos.SellerDto;
using Furniture.Services.Specifications;
using Microsoft.AspNetCore.Identity;
using System.Globalization;

namespace Furniture.Services
{
    public class SellerService : ISellerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public SellerService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public Task<SellerProfileDto?> GetSellerProfileByIdAsync(string sellerId, string language = "en") =>
            BuildSellerProfileAsync(sellerId, language, includeEmail: false);

        public Task<SellerProfileDto?> GetSellerProfileForCurrentUserAsync(string userId, string language = "en") =>
            BuildSellerProfileAsync(userId, language, includeEmail: true);

        public async Task<bool> UpdateSellerProfileAsync(string userId, UpdateSellerProfileDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null || user.Role != Roles.seller)
                return false;

            if (dto.Name != null)
                user.Name = dto.Name;

            if (dto.Location != null)
                user.Address = dto.Location;

            if (dto.ProfileImageUrl != null)
                user.ProfileImage = string.IsNullOrWhiteSpace(dto.ProfileImageUrl) ? null : dto.ProfileImageUrl.Trim();

            var profileRepo = _unitOfWork.GetRepository<SellerProfile, int>();
            var sellerProfile = await profileRepo.GetByIdAsync(new SellerProfileByUserIdSpecification(userId));

            if (sellerProfile is null)
            {
                var storeName = dto.Name ?? user.Name ?? "Seller";
                sellerProfile = new SellerProfile
                {
                    UserId = userId,
                    StoreName = storeName.Length > 200 ? storeName[..200] : storeName,
                    StoreDescription = dto.Bio,
                    BankName = dto.BankName,
                    BankAccountNumber = dto.BankAccountNumber,
                    BankCode = dto.BankCode,
                    NationalId = dto.NationalId,
                    PaymobMerchantId = dto.PaymobMerchantId,
                    CommissionRate = 6m,
                    IsVerified = false,
                    CreatedAt = DateTime.UtcNow
                };
                await profileRepo.AddAsync(sellerProfile);
            }
            else
            {
                if (dto.Bio != null)
                    sellerProfile.StoreDescription = dto.Bio;
                if (dto.Name != null)
                {
                    var storeName = dto.Name;
                    if (storeName.Length > 200)
                        storeName = storeName[..200];
                    sellerProfile.StoreName = storeName;
                }

                if (dto.BankName != null)
                    sellerProfile.BankName = dto.BankName;
                if (dto.BankAccountNumber != null)
                    sellerProfile.BankAccountNumber = dto.BankAccountNumber;
                if (dto.BankCode != null)
                    sellerProfile.BankCode = dto.BankCode;
                if (dto.NationalId != null)
                    sellerProfile.NationalId = dto.NationalId;
                if (dto.PaymobMerchantId != null)
                    sellerProfile.PaymobMerchantId = dto.PaymobMerchantId;

                profileRepo.Update(sellerProfile);
            }

            var identityResult = await _userManager.UpdateAsync(user);
            if (!identityResult.Succeeded)
                return false;

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        private async Task<SellerProfileDto?> BuildSellerProfileAsync(string sellerId, string language, bool includeEmail)
        {
            var user = await _unitOfWork
                .GetRepository<ApplicationUser, string>()
                .GetByIdAsync(sellerId);

            if (user is null || user.Role != Roles.seller)
                return null;

            var sellerProfile = await _unitOfWork
                .GetRepository<SellerProfile, int>()
                .GetByIdAsync(new SellerProfileByUserIdSpecification(sellerId));

            var products = (await _unitOfWork
                .GetRepository<Product, int>()
                .GetAllAsync(new SellerPortfolioProductsSpecification(sellerId)))
                .ToList();

            var allReviews = products
                .SelectMany(p => p.Reviews ?? Enumerable.Empty<Review>())
                .ToList();

            var reviewsCount = allReviews.Count;
            var averageRating = reviewsCount > 0 ? Math.Round((decimal)allReviews.Average(r => r.Rating), 1) : 0m;

            var completedOrders = products
                .SelectMany(p => p.OrderItems ?? Enumerable.Empty<OrderItem>())
                .Where(oi =>
                    oi.Order != null &&
                    (oi.Order.Status == OrderStatus.Delivered || oi.Order.Status == OrderStatus.Completed))
                .Select(oi => oi.OrderId)
                .Distinct()
                .Count();

            var specialties = products
                .Select(p => LocalizationHelper.Localize(p.Category?.NameEn, p.Category?.NameAr, language))
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct()
                .Take(8)
                .ToList();

            var portfolio = products
                .Take(6)
                .Select(p => new SellerPortfolioItemDto
                {
                    Id = p.Id,
                    Category = LocalizationHelper.Localize(p.Category?.NameEn, p.Category?.NameAr, language).ToUpperInvariant(),
                    Title = LocalizationHelper.Localize(p.NameEn, p.NameAr, language),
                    Description = LocalizationHelper.LocalizeNullable(p.DescriptionEn, p.DescriptionAr, language) ?? string.Empty,
                    ImageUrl = p.Images?.FirstOrDefault()?.ImageUrl ?? string.Empty
                })
                .ToList();

            var imageUrl = user.ProfileImage ?? string.Empty;

            return new SellerProfileDto
            {
                Id = user.Id,
                SellerId = user.Id,
                Name = user.Name ?? sellerProfile?.StoreName ?? string.Empty,
                Email = includeEmail ? user.Email : null,
                Location = user.Address ?? string.Empty,
                JoinDate = user.RegisteredAt.ToString("MMMM yyyy", CultureInfo.InvariantCulture),
                Rating = averageRating,
                ReviewsCount = reviewsCount,
                CompletedOrders = completedOrders,
                Bio = sellerProfile?.StoreDescription ?? string.Empty,
                AvatarUrl = imageUrl,
                ProfileImageUrl = imageUrl,
                Specialties = specialties,
                Portfolio = portfolio,
                BankName = includeEmail ? sellerProfile?.BankName : null,
                BankAccountNumber = includeEmail ? sellerProfile?.BankAccountNumber : null,
                BankCode = includeEmail ? sellerProfile?.BankCode : null,
                NationalId = includeEmail ? sellerProfile?.NationalId : null,
                PaymobMerchantId = includeEmail ? sellerProfile?.PaymobMerchantId : null,
                PendingCommission = includeEmail ? sellerProfile?.PendingCommission ?? 0m : 0m,
                MaxAllowedCommission = includeEmail ? sellerProfile?.MaxAllowedCommission ?? 0m : 0m,
                IsBlocked = includeEmail && (sellerProfile?.IsBlocked ?? false),
                BlockReason = includeEmail ? sellerProfile?.BlockReason : null
            };
        }
    }
}
