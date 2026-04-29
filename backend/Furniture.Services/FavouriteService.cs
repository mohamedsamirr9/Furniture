using AutoMapper;
using Furniture.Domain.InterfacesRepositories;
using Furniture.Domain.Models;
using Furniture.Services.Specifications;
using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos.FavouriteProductDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Services
{
    public class FavouriteService : IFavouriteService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IRecommendationService _recommendationService;

        public  FavouriteService(IUnitOfWork unitOfWork , IMapper mapper,  IRecommendationService recommendationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _recommendationService = recommendationService;
        }
        public async Task<FavouriteDto> AddToFavouritesAsync(string userId, int productId)
{
    var product = await _unitOfWork.GetRepository<Product, int>().GetByIdAsync(productId);

    if (product == null)
        throw new KeyNotFoundException("Product Not Found");

    var existingSpec = new FavouriteByUserAndProductSpecification(userId, productId);
    var existing = (await _unitOfWork.GetRepository<Favourite, int>()
        .GetAllAsync(existingSpec)).FirstOrDefault();

    if (existing != null)
        throw new InvalidOperationException("Product Is Already In Favourites");

    var favourite = new Favourite
    {
        UserId = userId,
        ProductId = productId
    };

    await _unitOfWork.GetRepository<Favourite, int>().AddAsync(favourite);
    await _unitOfWork.SaveChangesAsync();

    // 🔥 recommendation (run safely once)
    _ = Task.Run(async () =>
    {
        try
        {
            await _recommendationService.UpdateUserEmbeddingAsync(userId, productId, "favorite");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to update recommendation embedding: {ex.Message}");
        }
    });

    var specs = new FavouritesByUserSpecification(userId);
    var prods = await _unitOfWork.GetRepository<Favourite, int>().GetAllAsync(specs);

    var added = prods.First(f => f.ProductId == productId);

    return _mapper.Map<FavouriteDto>(added);
}

        public async Task<IEnumerable<FavouriteDto>> GetFavouritesAsync(string userId)
        {
            var spec = new FavouritesByUserSpecification(userId);

            var favourites = await _unitOfWork.GetRepository<Favourite, int>().GetAllAsync(spec);

            return _mapper.Map<IEnumerable<FavouriteDto>>(favourites);
        }

        public async Task RemoveFromFavouritesAsync(string userId, int productId)
        {
            var spec = new FavouriteByUserAndProductSpecification(userId, productId);

            var favourite = await _unitOfWork.GetRepository<Favourite, int>().GetByIdAsync(spec);

            if (favourite == null) throw new KeyNotFoundException("Product Is Not In Favourites");

            _unitOfWork.GetRepository<Favourite, int>().Remove(favourite);

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
