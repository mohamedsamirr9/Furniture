using Furniture.shared.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Servises_Abstraction
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryListDto>> GetAllCategoriesAsync(int pageIndex, int pageSize, string? search);

        Task<CategoryDto> GetCategoryByIdAsync(int id);

        Task<CategoryDto> CreateCategoryAsync(CategoryCreateUpdateDto dto);

        Task UpdateCategoryAsync(int id, CategoryCreateUpdateDto dto);

        Task DeleteCategoryAsync(int id);
    }
}
