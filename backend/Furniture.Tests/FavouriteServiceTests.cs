using AutoMapper;
using Furniture.Domain.InterfacesRepositories;
using Furniture.Domain.Models;
using Furniture.Services;
using Furniture.Services.Specifications;
using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos.FavouriteProductDto;
using Moq;
using FluentAssertions;

namespace Furniture.Tests
{
    public class FavouriteServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IRecommendationService> _mockRecommendationService;
        private readonly Mock<IGenaricRepository<Product, int>> _mockProductRepository;
        private readonly Mock<IGenaricRepository<Favourite, int>> _mockFavouriteRepository;
        private readonly FavouriteService _favouriteService;

        public FavouriteServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockRecommendationService = new Mock<IRecommendationService>();
            _mockProductRepository = new Mock<IGenaricRepository<Product, int>>();
            _mockFavouriteRepository = new Mock<IGenaricRepository<Favourite, int>>();

            _mockUnitOfWork
                .Setup(u => u.GetRepository<Product, int>())
                .Returns(_mockProductRepository.Object);

            _mockUnitOfWork
                .Setup(u => u.GetRepository<Favourite, int>())
                .Returns(_mockFavouriteRepository.Object);

            _favouriteService = new FavouriteService(
                _mockUnitOfWork.Object,
                _mockMapper.Object,
                _mockRecommendationService.Object
            );
        }

        #region GetFavouritesAsync Tests

        [Fact]
        public async Task GetFavouritesAsync_WithUserFavourites_ReturnsFavouriteDtos()
        {
            // Arrange
            var userId = "user-1";
            var favourites = new List<Favourite>
            {
                new Favourite { UserId = userId, ProductId = 1 },
                new Favourite { UserId = userId, ProductId = 2 }
            };

            var expectedDtos = new List<FavouriteDto>
            {
                new FavouriteDto { ProductId = 1 },
                new FavouriteDto { ProductId = 2 }
            };

            _mockFavouriteRepository
                .Setup(r => r.GetAllAsync(It.IsAny<ISpecifications<Favourite, int>>()))
                .ReturnsAsync(favourites);

            _mockMapper
                .Setup(m => m.Map<IEnumerable<FavouriteDto>>(It.IsAny<List<Favourite>>()))
                .Returns(expectedDtos);

            // Act
            var result = await _favouriteService.GetFavouritesAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            _mockFavouriteRepository.Verify(
                r => r.GetAllAsync(It.IsAny<ISpecifications<Favourite, int>>()),
                Times.Once
            );
        }

        [Fact]
        public async Task GetFavouritesAsync_WithNoFavourites_ReturnsEmptyList()
        {
            // Arrange
            var userId = "user-1";

            _mockFavouriteRepository
                .Setup(r => r.GetAllAsync(It.IsAny<ISpecifications<Favourite, int>>()))
                .ReturnsAsync(Enumerable.Empty<Favourite>());

            _mockMapper
                .Setup(m => m.Map<IEnumerable<FavouriteDto>>(It.IsAny<IEnumerable<Favourite>>()))
                .Returns(Enumerable.Empty<FavouriteDto>());

            // Act
            var result = await _favouriteService.GetFavouritesAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        #endregion

        #region AddToFavouritesAsync Tests

        [Fact]
        public async Task AddToFavouritesAsync_WithValidProductId_AddsFavourite()
        {
            // Arrange
            var userId = "user-1";
            var productId = 1;

            var product = new Product { Id = productId, NameEn = "Chair" };

            var newFavourite = new Favourite { UserId = userId, ProductId = productId };

            var expectedDto = new FavouriteDto { ProductId = productId };

            _mockProductRepository
                .Setup(r => r.GetByIdAsync(productId))
                .ReturnsAsync(product);

            // Setup GetAllAsync to return empty on first call (checking for existing), then with favourite on second call
            _mockFavouriteRepository
                .SetupSequence(r => r.GetAllAsync(It.IsAny<ISpecifications<Favourite, int>>()))
                .ReturnsAsync(Enumerable.Empty<Favourite>()) // First call: Check existing
                .ReturnsAsync(new List<Favourite> { newFavourite }); // Second call: Get all after adding

            _mockFavouriteRepository
                .Setup(r => r.AddAsync(It.IsAny<Favourite>()))
                .Returns(Task.CompletedTask);

            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            _mockRecommendationService
                .Setup(s => s.UpdateUserEmbeddingAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);


            _mockMapper
                .Setup(m => m.Map<FavouriteDto>(newFavourite))
                .Returns(expectedDto);

            // Act
            var result = await _favouriteService.AddToFavouritesAsync(userId, productId);

            // Assert
            result.Should().NotBeNull();
            result.ProductId.Should().Be(productId);
            _mockFavouriteRepository.Verify(r => r.AddAsync(It.IsAny<Favourite>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task AddToFavouritesAsync_WithNonExistentProduct_ThrowsException()
        {
            // Arrange
            var userId = "user-1";
            var productId = 999;

            _mockProductRepository
                .Setup(r => r.GetByIdAsync(productId))
                .ReturnsAsync((Product?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _favouriteService.AddToFavouritesAsync(userId, productId)
            );

            _mockFavouriteRepository.Verify(r => r.AddAsync(It.IsAny<Favourite>()), Times.Never);
        }

        [Fact]
        public async Task AddToFavouritesAsync_WithAlreadyFavouritedProduct_ThrowsException()
        {
            // Arrange
            var userId = "user-1";
            var productId = 1;

            var product = new Product { Id = productId };
            var existingFavourite = new Favourite { UserId = userId, ProductId = productId };

            _mockProductRepository
                .Setup(r => r.GetByIdAsync(productId))
                .ReturnsAsync(product);

            _mockFavouriteRepository
                .Setup(r => r.GetAllAsync(It.IsAny<ISpecifications<Favourite, int>>()))
                .ReturnsAsync(new List<Favourite> { existingFavourite });

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _favouriteService.AddToFavouritesAsync(userId, productId)
            );

            _mockFavouriteRepository.Verify(r => r.AddAsync(It.IsAny<Favourite>()), Times.Never);
        }

        #endregion

        #region RemoveFromFavouritesAsync Tests

        [Fact]
        public async Task RemoveFromFavouritesAsync_WithValidFavourite_RemovesFavourite()
        {
            // Arrange
            var userId = "user-1";
            var productId = 1;

            var favourite = new Favourite { UserId = userId, ProductId = productId };

            _mockFavouriteRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<ISpecifications<Favourite, int>>()))
                .ReturnsAsync(favourite);

            _mockFavouriteRepository
                .Setup(r => r.Remove(It.IsAny<Favourite>()))
                .Callback<Favourite>(f => { }); // No-op callback

            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _favouriteService.RemoveFromFavouritesAsync(userId, productId);

            // Assert
            _mockFavouriteRepository.Verify(r => r.Remove(It.IsAny<Favourite>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task RemoveFromFavouritesAsync_WithNonExistentFavourite_ThrowsException()
        {
            // Arrange
            var userId = "user-1";
            var productId = 999;

            _mockFavouriteRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<ISpecifications<Favourite, int>>()))
                .ReturnsAsync((Favourite?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _favouriteService.RemoveFromFavouritesAsync(userId, productId)
            );

            _mockFavouriteRepository.Verify(r => r.Remove(It.IsAny<Favourite>()), Times.Never);
        }

        #endregion
    }
}

