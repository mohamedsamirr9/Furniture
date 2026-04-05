using AutoMapper;
using Furniture.Domain.InterfacesRepositories;
using Furniture.Domain.Models;
using Furniture.Services.Specifications;
using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos.CategoryDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Services
{
    public class CategoryService(IUnitOfWork _unitOfWork, IMapper _mapper) : ICategoryService
    {
        public async Task<CategoryDto> CreateCategoryAsync(CategoryCreateUpdateDto dto)
        {
            var repo = _unitOfWork.GetRepository<Category, int>();
            var category = _mapper.Map<Category>(dto);
            await repo.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<Category, CategoryDto>(category);
        }

        public async Task DeleteCategoryAsync(int id)
        {
            var repo = _unitOfWork.GetRepository<Category, int>();
            var category = await repo.GetByIdAsync(id);
            if (category is null) return;

            repo.Remove(category);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<CategoryListDto>> GetAllCategoriesAsync(int pageIndex, int pageSize, string? search)
        {
            var repo = _unitOfWork.GetRepository<Category, int>();

            var spec = new CategorySpecifications(pageIndex, pageSize, search);

            var categories = await repo.GetAllAsync(spec);

            return _mapper.Map<IEnumerable<Category>, IEnumerable<CategoryListDto>>(categories);
        }

        public async Task<CategoryDto> GetCategoryByIdAsync(int id)
        {
            var spec = new CategoryWithProductsSpecifications(id);
            var category = await _unitOfWork.GetRepository<Category, int>().GetByIdAsync(spec);

            if (category is null) throw new Exception($"Category with id {id} not found");
            return _mapper.Map<Category, CategoryDto>(category);
        }

        public async Task UpdateCategoryAsync(int id, CategoryCreateUpdateDto dto)
        {
            var repo = _unitOfWork.GetRepository<Category, int>();
            var category = await repo.GetByIdAsync(id);
            if (category is null) throw new Exception($"Category with id {id} not found");
             _mapper.Map(dto, category);
              repo.Update(category);

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
