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
        Task<IEnumerable<ProductListDto>> GetAllAsync(int pageIndex, int pageSize, string? search);
        Task<ProductDetailsDto?> GetByIdAsync(int id);
        Task<ProductDetailsDto> CreateAsync(ProductCreateUpdateDto dto);
        Task UpdateAsync(int id, ProductCreateUpdateDto dto);
        Task DeleteAsync(int id);
    }
}
