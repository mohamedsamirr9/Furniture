using Furniture.shared.Dtos.CategoryDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Servises_Abstraction
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryListDto>> GetAllCategoriesAsync(int pageIndex, int pageSize, string? search, string language = "en");

        Task<CategoryDto> GetCategoryByIdAsync(int id, string language = "en");

        Task<CategoryDto> CreateCategoryAsync(CategoryCreateUpdateDto dto, string language = "en");

        Task UpdateCategoryAsync(int id, CategoryCreateUpdateDto dto);

        Task DeleteCategoryAsync(int id);
    }
}
