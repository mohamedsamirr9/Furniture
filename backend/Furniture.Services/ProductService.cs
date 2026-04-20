using AutoMapper;
using Furniture.Domain.InterfacesRepositories;
using Furniture.Domain.Models;
using Furniture.Services.Specifications;
using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos.ProductDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Furniture.Servises_Abstraction.Exceptions;

namespace Furniture.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IImageValidationService _imageValidationService;
        private readonly IRecommendationService _recommendationService;

        private const int MaxProductImages = 5;

        public ProductService(IUnitOfWork unitOfWork , IMapper mapper, IImageValidationService imageValidationService,  IRecommendationService recommendationService)
        {
            _unitOfWork = unitOfWork;
            _recommendationService  = recommendationService;
            _mapper = mapper;
            _imageValidationService = imageValidationService;
        }

        // public async Task<ProductDetailsDto> CreateAsync(ProductCreateUpdateDto dto, string language = "en")
        // {
        //     var repo = _unitOfWork.GetRepository<Product, int>();
        //     var product = _mapper.Map<Product>(dto);
        //     product.CreatedAt = DateTime.UtcNow;
        //
        //     if (dto.ImageUrls != null && dto.ImageUrls.Any())
        //     {
        //         foreach (var url in dto.ImageUrls)
        //         {
        //             product.Images.Add(new ProductImage { ImageUrl = url });
        //         }
        //     }
        //
        //     await repo.AddAsync(product);
        //     await _unitOfWork.SaveChangesAsync();
        //
        //     var result = _mapper.Map<ProductDetailsDto>(product);
        //     LocalizeProductDetails(product, result, language);
        //     return result;
        // }
        
        public async Task<ProductDetailsDto> CreateAsync(ProductCreateUpdateDto dto, string language = "en")
        {
            if (dto.ImageUrls != null && dto.ImageUrls.Any())
            {
                var summary = await _imageValidationService.ValidateUrlsAsync(dto.ImageUrls);
                if (!summary.AllApproved)
                    throw new ImageValidationException(summary); 
            }
            
            var repo = _unitOfWork.GetRepository<Product, int>();
            var product = _mapper.Map<Product>(dto);
            product.CreatedAt = DateTime.UtcNow;

            if (dto.ImageUrls != null && dto.ImageUrls.Count > MaxProductImages)
                throw new Exception($"A product can have at most {MaxProductImages} images.");

            foreach (var url in dto.ImageUrls ?? Enumerable.Empty<string>())
                product.Images.Add(new ProductImage { ImageUrl = url });

            await repo.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            var result = _mapper.Map<ProductDetailsDto>(product);
            LocalizeProductDetails(product, result, language);
            return result;
        }

        public async Task UpdateAsync(int id, ProductCreateUpdateDto dto)
        {
            
            if (dto.ImageUrls != null && dto.ImageUrls.Any())
            {
                var summary = await _imageValidationService.ValidateUrlsAsync(dto.ImageUrls);
                if (!summary.AllApproved)
                    throw new ImageValidationException(summary);
            }
            var repo = _unitOfWork.GetRepository<Product, int>();
            var spec = new ProductWithDetailsSpecifications(id);
            var product = await repo.GetByIdAsync(spec);

            if (product is null) throw new Exception($"Product with id {id} not found");

            _mapper.Map(dto, product);
            product.Images.Clear();

            foreach (var url in dto.ImageUrls ?? Enumerable.Empty<string>())
                product.Images.Add(new ProductImage { ImageUrl = url });

            repo.Update(product);
            await _unitOfWork.SaveChangesAsync();
        }

        

        public async Task DeleteAsync(int id)
        {
            var repo = _unitOfWork.GetRepository<Product, int>();
            var product = await repo.GetByIdAsync(id);
            if (product is null) throw new Exception($"Product with id {id} not found");

            await _recommendationService.DeleteProductEmbeddingAsync(id);

            repo.Remove(product);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<PaginatedProductsDto> GetAllAsync(ProductQueryParams queryParams, string language = "en")
        {
            var repo = _unitOfWork.GetRepository<Product, int>();

            var countSpec = new ProductCountSpecification(queryParams);
            var totalCount = await repo.CountAsync(countSpec);

            var spec = new ProductSpecifications(queryParams);
            var products = await repo.GetAllAsync(spec);

            var productList = products.ToList();
            var data = _mapper.Map<List<ProductListDto>>(productList);

            for (int i = 0; i < data.Count; i++)
            {
                LocalizeProductList(productList[i], data[i], language);
            }

            return new PaginatedProductsDto
            {
                TotalCount = totalCount,
                Page = queryParams.Page,
                PageSize = queryParams.PageSize,
                Data = data
            };
        }

        public async Task<PaginatedProductsDto> GetSellerProductsAsync(string sellerId, ProductQueryParams queryParams, string language = "en")
        {
            var repo = _unitOfWork.GetRepository<Product, int>();

            var countSpec = new SellerProductsCountSpecification(sellerId, queryParams);
            var totalCount = await repo.CountAsync(countSpec);

            var spec = new SellerProductsSpecification(sellerId, queryParams);
            var products = await repo.GetAllAsync(spec);

            var productList = products.ToList();
            var data = _mapper.Map<List<ProductListDto>>(productList);

            for (int i = 0; i < data.Count; i++)
            {
                LocalizeProductList(productList[i], data[i], language);
            }

            return new PaginatedProductsDto
            {
                TotalCount = totalCount,
                Page = queryParams.Page,
                PageSize = queryParams.PageSize,
                Data = data
            };
        }

        public async Task<ProductDetailsDto?> GetByIdAsync(int id, string language = "en")
        {
            var repo = _unitOfWork.GetRepository<Product, int>();
            var spec = new ProductWithDetailsSpecifications(id);
            var product = await repo.GetByIdAsync(spec);

            if (product is null) return null;

            var result = _mapper.Map<ProductDetailsDto>(product);
            LocalizeProductDetails(product, result, language);
            return result;
        }

        public async Task UpdateAsync(int id, ProductCreateUpdateDto dto)
        {
            var repo = _unitOfWork.GetRepository<Product, int>();
            var spec = new ProductWithDetailsSpecifications(id);
            var product = await repo.GetByIdAsync(spec);

            if (product is null) throw new Exception($"Product with id {id} not found");

            if (dto.ImageUrls != null && dto.ImageUrls.Count > MaxProductImages)
                throw new Exception($"A product can have at most {MaxProductImages} images.");

            _mapper.Map(dto, product);

            product.Images.Clear();
            if (dto.ImageUrls != null && dto.ImageUrls.Any())
            {
                foreach (var url in dto.ImageUrls)
                {
                    product.Images.Add(new ProductImage { ImageUrl = url });
                }
            }

            repo.Update(product);
            await _unitOfWork.SaveChangesAsync();
        }

        private static void LocalizeProductList(Product entity, ProductListDto dto, string language)
        {
            dto.Name = LocalizationHelper.Localize(entity.NameEn, entity.NameAr, language);
            if (entity.Category != null)
            {
                dto.CategoryName = LocalizationHelper.Localize(
                    entity.Category.NameEn, entity.Category.NameAr, language);
            }
        }

        private static void LocalizeProductDetails(Product entity, ProductDetailsDto dto, string language)
        {
            dto.Name = LocalizationHelper.Localize(entity.NameEn, entity.NameAr, language);
            dto.Description = LocalizationHelper.LocalizeNullable(entity.DescriptionEn, entity.DescriptionAr, language);
            if (entity.Category != null)
            {
                dto.CategoryName = LocalizationHelper.Localize(
                    entity.Category.NameEn, entity.Category.NameAr, language);
            }
        }
    }
}
