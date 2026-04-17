using Furniture.Domain.InterfacesRepositories;
using Furniture.Domain.Models;
using Furniture.Domain.Models.Enum;
using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos.SellerDto;
using Furniture.Services.Specifications;
using System.Globalization;

namespace Furniture.Services
{
    public class SellerService : ISellerService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SellerService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<SellerProfileDto?> GetSellerProfileByIdAsync(string sellerId, string language = "en")
        {
            var user = await _unitOfWork
                .GetRepository<ApplicationUser, string>()
                .GetByIdAsync(sellerId);

            if (user is null || user.Role != Roles.seller)
            {
                return null;
            }

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

            return new SellerProfileDto
            {
                Id = user.Id,
                Name = user.Name ?? sellerProfile?.StoreName ?? string.Empty,
                Location = user.Address ?? string.Empty,
                JoinDate = user.RegisteredAt.ToString("MMMM yyyy", CultureInfo.InvariantCulture),
                Rating = averageRating,
                ReviewsCount = reviewsCount,
                CompletedOrders = completedOrders,
                Bio = sellerProfile?.StoreDescription ?? string.Empty,
                AvatarUrl = user.ProfileImage ?? string.Empty,
                Specialties = specialties,
                Portfolio = portfolio
            };
        }
    }
}
