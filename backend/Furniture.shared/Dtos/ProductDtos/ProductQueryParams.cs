using System;

namespace Furniture.shared.Dtos.ProductDtos
{
    public class ProductQueryParams
    {
        public int? CategoryId { get; set; }
        public string? Search { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? Sort { get; set; }

        private int _page = 1;
        public int Page
        {
            get => _page;
            set => _page = Math.Max(1, value);
        }

        private int _pageSize = 10;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = Math.Clamp(value, 1, 50);
        }
    }
}
