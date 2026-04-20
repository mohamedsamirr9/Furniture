using AutoMapper;
using Furniture.Domain.InterfacesRepositories;
using Furniture.Domain.Models;
using Furniture.Servises_Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Services
{
    public class ProductImageService : IProductImageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IImageValidationService _imageValidationService;
        private readonly IRecommendationService _recommendationService;


        public ProductImageService(IUnitOfWork unitOfWork , IMapper mapper,IRecommendationService recommendationService, IImageValidationService imageValidationService )
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _imageValidationService = imageValidationService;
            _recommendationService = recommendationService;
        }
        // public async Task AddImageAsync(int productId, string imageUrl)
        // {
        //     var productRepo = _unitOfWork.GetRepository<Product, int>();
        //     var imageRepo = _unitOfWork.GetRepository<ProductImage, int>();
        //
        //     var product = await productRepo.GetByIdAsync(productId);
        //     if (product is null) throw new Exception($"Product with id {productId} not found");
        //
        //     var image = new ProductImage
        //     {
        //         ImageUrl = imageUrl,
        //         ProductId = productId
        //     };
        //
        //     await imageRepo.AddAsync(image);
        //     await _unitOfWork.SaveChangesAsync();
        // }
        public async Task AddImageAsync(int productId, Stream imageStream, string fileName, string contentType)
        {
            var productRepo = _unitOfWork.GetRepository<Product, int>();
            var imageRepo = _unitOfWork.GetRepository<ProductImage, int>();

            var product = await productRepo.GetByIdAsync(productId);
            if (product is null) 
                throw new Exception($"Product with id {productId} not found");

            imageStream.Position = 0;
            var validationResult = await _imageValidationService.ValidateAsync(imageStream, fileName, contentType);

            if (validationResult.Decision == "reject")
            {
                throw new Exception(
                    $"Image '{fileName}' was rejected. AI probability: {validationResult.AiProbability:P0}"
                );
            }

            imageStream.Position = 0;
            string imageUrl = $"/uploads/products/{Guid.NewGuid()}_{fileName}";

            var image = new ProductImage
            {
                ImageUrl = imageUrl,
                ProductId = productId
            };

            await imageRepo.AddAsync(image);
            await _unitOfWork.SaveChangesAsync();
            
            _ = _recommendationService.GenerateAndSaveEmbeddingAsync(
                productId, imageUrl, product.DescriptionEn);
        }
        
        

        public async Task DeleteImageAsync(int imageId)
        {
            var repo = _unitOfWork.GetRepository<ProductImage, int>();

            var image = await repo.GetByIdAsync(imageId);

            if (image is null) throw new Exception($"Image with id {imageId} not found");

            repo.Remove(image);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
