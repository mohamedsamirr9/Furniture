using Furniture.Domain.Models;
using Furniture.shared.Dtos.ProductDtos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq.Expressions;

namespace Furniture.Services.Specifications
{
    public static class ProductFilters
    {
        public static Expression<Func<Product, bool>> BuildFilter(ProductQueryParams queryParams)
        {
            var search = queryParams.Search?.ToLower();

            return p =>
                (string.IsNullOrWhiteSpace(search) || EF.Functions.Like(p.NameEn.ToLower(), $"%{search}%")) &&
                (!queryParams.CategoryId.HasValue || p.CategoryId == queryParams.CategoryId) &&
                (!queryParams.MinPrice.HasValue || p.Price >= queryParams.MinPrice.Value) &&
                (!queryParams.MaxPrice.HasValue || p.Price <= queryParams.MaxPrice.Value);
        }
    }

    public class ProductSpecifications : BaseSpecificationscs<Product, int>
    {
        public ProductSpecifications(ProductQueryParams queryParams)
            : base(ProductFilters.BuildFilter(queryParams))
        {
            AddInclude(p => p.Category);
            AddInclude(p => p.Seller);
            AddInclude(p => p.Images);
            AddInclude(p => p.Reviews);

            if (!string.IsNullOrWhiteSpace(queryParams.Sort))
            {
                switch (queryParams.Sort.ToLower())
                {
                    case "priceasc":
                        AddOrderBy(p => p.Price);
                        break;
                    case "pricedesc":
                        AddOrderByDescending(p => p.Price);
                        break;
                    case "newest":
                    default:
                        AddOrderByDescending(p => p.CreatedAt);
                        break;
                }
            }
            else
            {
                AddOrderByDescending(p => p.CreatedAt);
            }

            ApplyPagination(queryParams.PageSize, queryParams.Page);
        }
    }
}
