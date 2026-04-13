using Furniture.Domain.InterfacesRepositories;
using Furniture.Domain.Models;
using Furniture.Servises_Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Furniture.Services
{
    public class ShippingCalculatorService : IShippingCalculatorService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICartService _cartService;
        private const decimal FALLBACK_SHIPPING_COST = 50m;

        public ShippingCalculatorService(IUnitOfWork unitOfWork, ICartService cartService)
        {
            _unitOfWork = unitOfWork;
            _cartService = cartService;
        }

        public async Task<(decimal ShippingCost, int? ShippingRuleId)> CalculateShippingAsync(string city, IEnumerable<int> categoryIds)
        {
            if (string.IsNullOrWhiteSpace(city))
                return (FALLBACK_SHIPPING_COST, null);

            var categoryIdList = categoryIds?.ToList();

            // Custom offers or empty cart
            if (categoryIdList == null || !categoryIdList.Any())
                return (FALLBACK_SHIPPING_COST, null);

            var repo = _unitOfWork.GetRepository<ShippingRule, int>();
            var cityLower = city.ToLower();

            var allRules = await repo.GetAllAsync();
            var applicableRules = allRules
                .Where(r => r.City.ToLower() == cityLower && categoryIdList.Contains(r.CategoryId))
                .ToList();

            if (!applicableRules.Any())
                return (FALLBACK_SHIPPING_COST, null);

            var highestRule = applicableRules.OrderByDescending(r => r.Price).First();
            
            return (highestRule.Price, highestRule.Id);
        }

        public async Task<decimal> GetShippingEstimateAsync(string userId, string city, int? offerId)
        {
            if (offerId.HasValue && offerId > 0)
            {
                var result = await CalculateShippingAsync(city, new List<int>());
                return result.ShippingCost;
            }

            var cart = await _cartService.GetCartAsync(userId);
            if (cart == null || !cart.Items.Any())
                return 0;

            var productIds = cart.Items.Select(i => i.ProductId).ToList();
            var categoryIds = new List<int>();

            if (productIds.Any())
            {
                var productRepo = _unitOfWork.GetRepository<Product, int>();
                var products = await productRepo.GetAllAsync();
                categoryIds = products
                    .Where(p => productIds.Contains(p.Id))
                    .Select(p => p.CategoryId)
                    .Distinct()
                    .ToList();
            }

            var shippingResult = await CalculateShippingAsync(city, categoryIds);
            return shippingResult.ShippingCost;
        }
    }
}
