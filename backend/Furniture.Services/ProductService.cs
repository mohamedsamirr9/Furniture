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

namespace Furniture.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductService(IUnitOfWork unitOfWork , IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<ProductDetailsDto> CreateAsync(ProductCreateUpdateDto dto)
        {
            var repo = _unitOfWork.GetRepository<Product, int>();

            var product = _mapper.Map<Product>(dto);

            product.CreatedAt = DateTime.UtcNow;

            if (dto.ImageUrls != null && dto.ImageUrls.Any())
            {
                foreach (var url in dto.ImageUrls)
                {
                    product.Images.Add(new ProductImage { ImageUrl = url });
                }
            }

            await repo.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ProductDetailsDto>(product);
        }

        public async Task DeleteAsync(int id)
        {
            var repo = _unitOfWork.GetRepository<Product, int>();

            var product = await repo.GetByIdAsync(id);

            if (product is null) throw new Exception($"Product with id {id} not found");

            repo.Remove(product);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<PaginatedProductsDto> GetAllAsync(ProductQueryParams queryParams)
        {
            var repo = _unitOfWork.GetRepository<Product, int>();

            var countSpec = new ProductCountSpecification(queryParams);
            var totalCount = await repo.CountAsync(countSpec);

            var spec = new ProductSpecifications(queryParams);
            var products = await repo.GetAllAsync(spec);

            var data = _mapper.Map<IEnumerable<Product>, IEnumerable<ProductListDto>>(products);

            return new PaginatedProductsDto
            {
                TotalCount = totalCount,
                Page = queryParams.Page,
                PageSize = queryParams.PageSize,
                Data = data
            };
        }

        public async Task<ProductDetailsDto?> GetByIdAsync(int id)
        {
            var repo = _unitOfWork.GetRepository<Product, int>();

            var spec = new ProductWithDetailsSpecifications(id);

            var product = await repo.GetByIdAsync(spec);

            if (product is null)
                return null;

            return _mapper.Map<Product, ProductDetailsDto>(product);
        }

        public async Task UpdateAsync(int id, ProductCreateUpdateDto dto)
        {
            var repo = _unitOfWork.GetRepository<Product, int>();

            var spec = new ProductWithDetailsSpecifications(id);
            var product = await repo.GetByIdAsync(spec);

            if (product is null) throw new Exception($"Product with id {id} not found");

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
    }
}
