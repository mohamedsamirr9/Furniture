using System.Collections.Generic;

namespace Furniture.shared.Dtos.ProductDtos
{
    public class PaginatedProductsDto
    {
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public IEnumerable<ProductListDto> Data { get; set; } = new List<ProductListDto>();
    }
}
