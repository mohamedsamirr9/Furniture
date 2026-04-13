using Furniture.shared.Dtos.ProductDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Servises_Abstraction
{
    public interface IProductService 
    {
        Task<PaginatedProductsDto> GetAllAsync(ProductQueryParams queryParams, string language = "en");
        Task<ProductDetailsDto?> GetByIdAsync(int id, string language = "en");
        Task<ProductDetailsDto> CreateAsync(ProductCreateUpdateDto dto, string language = "en");
        Task UpdateAsync(int id, ProductCreateUpdateDto dto);
        Task DeleteAsync(int id);
    }
}
