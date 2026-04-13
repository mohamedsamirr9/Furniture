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
        public async Task<CategoryDto> CreateCategoryAsync(CategoryCreateUpdateDto dto, string language = "en")
        {
            var repo = _unitOfWork.GetRepository<Category, int>();
            var category = _mapper.Map<Category>(dto);
            await repo.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            var result = _mapper.Map<CategoryDto>(category);
            LocalizeCategoryDto(category, result, language);
            return result;
        }

        public async Task DeleteCategoryAsync(int id)
        {
            var repo = _unitOfWork.GetRepository<Category, int>();
            var category = await repo.GetByIdAsync(id);
            if (category is null) return;

            repo.Remove(category);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<CategoryListDto>> GetAllCategoriesAsync(int pageIndex, int pageSize, string? search, string language = "en")
        {
            var repo = _unitOfWork.GetRepository<Category, int>();
            var spec = new CategorySpecifications(pageIndex, pageSize, search);
            var categories = await repo.GetAllAsync(spec);

            var categoryList = categories.ToList();
            var data = _mapper.Map<List<CategoryListDto>>(categoryList);

            for (int i = 0; i < data.Count; i++)
            {
                data[i].Name = LocalizationHelper.Localize(
                    categoryList[i].NameEn, categoryList[i].NameAr, language);
            }

            return data;
        }

        public async Task<CategoryDto> GetCategoryByIdAsync(int id, string language = "en")
        {
            var spec = new CategoryWithProductsSpecifications(id);
            var category = await _unitOfWork.GetRepository<Category, int>().GetByIdAsync(spec);

            if (category is null) throw new Exception($"Category with id {id} not found");

            var result = _mapper.Map<CategoryDto>(category);
            LocalizeCategoryDto(category, result, language);

            if (category.Products != null)
            {
                var productList = category.Products.ToList();
                for (int i = 0; i < result.Products.Count; i++)
                {
                    result.Products[i].Name = LocalizationHelper.Localize(
                        productList[i].NameEn, productList[i].NameAr, language);
                    result.Products[i].CategoryName = result.Name;
                }
            }

            return result;
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

        private static void LocalizeCategoryDto(Category entity, CategoryDto dto, string language)
        {
            dto.Name = LocalizationHelper.Localize(entity.NameEn, entity.NameAr, language);
            dto.Description = LocalizationHelper.LocalizeNullable(entity.DescriptionEn, entity.DescriptionAr, language);
        }
    }
}
