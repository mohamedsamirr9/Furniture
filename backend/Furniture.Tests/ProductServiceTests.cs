using AutoMapper;
using Furniture.Domain.InterfacesRepositories;
using Furniture.Domain.Models;
using Furniture.Services;
using Furniture.Services.Specifications;
using Furniture.Servises_Abstraction;
using Furniture.Servises_Abstraction.Exceptions;
using Furniture.shared.Dtos.ProductDtos;
using Moq;
using FluentAssertions;
using ReviewDto = Furniture.shared.Dtos.ReviewDto;

namespace Furniture.Tests
{
    public class ProductServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IImageValidationService> _mockImageValidationService;
        private readonly Mock<IRecommendationService> _mockRecommendationService;
        private readonly Mock<IGenaricRepository<Product, int>> _mockProductRepository;
        private readonly ProductService _productService;

        public ProductServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockImageValidationService = new Mock<IImageValidationService>();
            _mockRecommendationService = new Mock<IRecommendationService>();
            _mockProductRepository = new Mock<IGenaricRepository<Product, int>>();

            _mockUnitOfWork
                .Setup(u => u.GetRepository<Product, int>())
                .Returns(_mockProductRepository.Object);

            _productService = new ProductService(
                _mockUnitOfWork.Object,
                _mockMapper.Object,
                _mockImageValidationService.Object,
                _mockRecommendationService.Object
            );
        }

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_WithValidId_ReturnsProductDetailsDto()
        {
            // Arrange
            var productId = 1;
            var language = "en";
            var product = new Product
            {
                Id = productId,
                NameEn = "Chair",
                NameAr = "كرسي",
                DescriptionEn = "A nice chair",
                DescriptionAr = "كرسي جميل",
                Price = 100,
                StockQuantity = 10,
                CategoryId = 1,
                Category = new Category { NameEn = "Furniture", NameAr = "أثاث" },
                CreatedAt = DateTime.UtcNow
            };

            var expectedDto = new ProductDetailsDto
            {
                Id = productId,
                Name = "Chair",
                NameEn = "Chair",
                NameAr = "كرسي",
                Description = "A nice chair",
                DescriptionEn = "A nice chair",
                DescriptionAr = "كرسي جميل",
                Price = 100,
                StockQuantity = 10,
                CategoryId = 1,
                CategoryName = "Furniture"
            };

            _mockProductRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<ISpecifications<Product, int>>()))
                .ReturnsAsync(product);

            _mockMapper
                .Setup(m => m.Map<ProductDetailsDto>(product))
                .Returns(expectedDto);

            // Act
            var result = await _productService.GetByIdAsync(productId, language);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(productId);
            result.Name.Should().Be("Chair");
            result.Price.Should().Be(100);
            _mockProductRepository.Verify(
                r => r.GetByIdAsync(It.IsAny<ISpecifications<Product, int>>()),
                Times.Once
            );
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            var productId = 999;
            var language = "en";

            _mockProductRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<ISpecifications<Product, int>>()))
                .ReturnsAsync((Product?)null);

            // Act
            var result = await _productService.GetByIdAsync(productId, language);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_WithArabicLanguage_LocalizesToArabic()
        {
            // Arrange
            var productId = 1;
            var language = "ar";
            var product = new Product
            {
                Id = productId,
                NameEn = "Chair",
                NameAr = "كرسي",
                DescriptionEn = "A nice chair",
                DescriptionAr = "كرسي جميل",
                Price = 100,
                StockQuantity = 10,
                CategoryId = 1,
                Category = new Category { NameEn = "Furniture", NameAr = "أثاث" },
                CreatedAt = DateTime.UtcNow
            };

            var expectedDto = new ProductDetailsDto
            {
                Id = productId,
                Name = "كرسي",
                NameEn = "Chair",
                NameAr = "كرسي",
                Description = "كرسي جميل",
                DescriptionEn = "A nice chair",
                DescriptionAr = "كرسي جميل",
                Price = 100,
                StockQuantity = 10,
                CategoryId = 1,
                CategoryName = "أثاث"
            };

            _mockProductRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<ISpecifications<Product, int>>()))
                .ReturnsAsync(product);

            _mockMapper
                .Setup(m => m.Map<ProductDetailsDto>(product))
                .Returns(expectedDto);

            // Act
            var result = await _productService.GetByIdAsync(productId, language);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("كرسي");
        }

        #endregion

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_WithValidQueryParams_ReturnsPaginatedProducts()
        {
            // Arrange
            var queryParams = new ProductQueryParams { Page = 1, PageSize = 10 };
            var language = "en";

            var products = new List<Product>
            {
                new Product
                {
                    Id = 1,
                    NameEn = "Chair",
                    NameAr = "كرسي",
                    DescriptionEn = "A nice chair",
                    Price = 100,
                    StockQuantity = 10,
                    CategoryId = 1,
                    Category = new Category { NameEn = "Furniture" },
                    CreatedAt = DateTime.UtcNow
                }
            };

            var productDtos = new List<ProductListDto>
            {
                new ProductListDto
                {
                    Id = 1,
                    Name = "Chair",
                    NameEn = "Chair",
                    Price = 100,
                    StockQuantity = 10,
                    CategoryId = 1,
                    CategoryName = "Furniture",
                    SellerName = "Seller1"
                }
            };

            _mockProductRepository
                .Setup(r => r.CountAsync(It.IsAny<ISpecifications<Product, int>>()))
                .ReturnsAsync(1);

            _mockProductRepository
                .Setup(r => r.GetAllAsync(It.IsAny<ISpecifications<Product, int>>()))
                .ReturnsAsync(products);

            _mockMapper
                .Setup(m => m.Map<List<ProductListDto>>(It.IsAny<List<Product>>()))
                .Returns(productDtos);

            // Act
            var result = await _productService.GetAllAsync(queryParams, language);

            // Assert
            result.Should().NotBeNull();
            result.TotalCount.Should().Be(1);
            result.Page.Should().Be(1);
            result.PageSize.Should().Be(10);
            result.Data.Should().HaveCount(1);
            _mockProductRepository.Verify(r => r.CountAsync(It.IsAny<ISpecifications<Product, int>>()), Times.Once);
            _mockProductRepository.Verify(r => r.GetAllAsync(It.IsAny<ISpecifications<Product, int>>()), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_WithEmptyResult_ReturnsEmptyPaginatedDto()
        {
            // Arrange
            var queryParams = new ProductQueryParams { Page = 1, PageSize = 10 };
            var language = "en";

            _mockProductRepository
                .Setup(r => r.CountAsync(It.IsAny<ISpecifications<Product, int>>()))
                .ReturnsAsync(0);

            _mockProductRepository
                .Setup(r => r.GetAllAsync(It.IsAny<ISpecifications<Product, int>>()))
                .ReturnsAsync(Enumerable.Empty<Product>());

            _mockMapper
                .Setup(m => m.Map<List<ProductListDto>>(It.IsAny<List<Product>>()))
                .Returns(new List<ProductListDto>());

            // Act
            var result = await _productService.GetAllAsync(queryParams, language);

            // Assert
            result.Should().NotBeNull();
            result.TotalCount.Should().Be(0);
            result.Data.Should().BeEmpty();
        }

        #endregion

        #region GetSellerProductsAsync Tests

        [Fact]
        public async Task GetSellerProductsAsync_WithValidSellerId_ReturnsSellerProducts()
        {
            // Arrange
            var sellerId = "seller-1";
            var queryParams = new ProductQueryParams { Page = 1, PageSize = 10 };
            var language = "en";

            var products = new List<Product>
            {
                new Product
                {
                    Id = 1,
                    NameEn = "Chair",
                    SellerId = sellerId,
                    CategoryId = 1,
                    Category = new Category { NameEn = "Furniture" },
                    CreatedAt = DateTime.UtcNow
                }
            };

            var productDtos = new List<ProductListDto>
            {
                new ProductListDto { Id = 1, Name = "Chair", CategoryName = "Furniture" }
            };

            _mockProductRepository
                .Setup(r => r.CountAsync(It.IsAny<ISpecifications<Product, int>>()))
                .ReturnsAsync(1);

            _mockProductRepository
                .Setup(r => r.GetAllAsync(It.IsAny<ISpecifications<Product, int>>()))
                .ReturnsAsync(products);

            _mockMapper
                .Setup(m => m.Map<List<ProductListDto>>(It.IsAny<List<Product>>()))
                .Returns(productDtos);

            // Act
            var result = await _productService.GetSellerProductsAsync(sellerId, queryParams, language);

            // Assert
            result.Should().NotBeNull();
            result.TotalCount.Should().Be(1);
            result.Data.Should().HaveCount(1);
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_WithValidDto_CreatesProduct()
        {
            // Arrange
            var dto = new ProductCreateUpdateDto
            {
                NameEn = "New Chair",
                DescriptionEn = "A new product",
                Price = 150,
                StockQuantity = 20,
                IsAvailable = true,
                CategoryId = 1,
                SellerId = "seller-1",
                ImageUrls = new List<string> { "https://example.com/image1.jpg" }
            };

            var validationSummary = new ImageValidationSummary
            {
                AllApproved = true,
                Results = new List<ImageUrlValidationResult>()
            };

            var product = new Product
            {
                Id = 1,
                NameEn = dto.NameEn,
                DescriptionEn = dto.DescriptionEn,
                Price = dto.Price,
                StockQuantity = dto.StockQuantity,
                CategoryId = dto.CategoryId,
                SellerId = dto.SellerId,
                CreatedAt = DateTime.UtcNow,
                Images = new List<ProductImage>()
            };

            var expectedDto = new ProductDetailsDto
            {
                Id = 1,
                NameEn = dto.NameEn,
                Name = dto.NameEn,
                DescriptionEn = dto.DescriptionEn,
                Description = dto.DescriptionEn,
                Price = dto.Price,
                StockQuantity = dto.StockQuantity
            };

            _mockImageValidationService
                .Setup(s => s.ValidateUrlsAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(validationSummary);

            _mockMapper
                .Setup(m => m.Map<Product>(dto))
                .Returns(product);

            _mockProductRepository
                .Setup(r => r.AddAsync(It.IsAny<Product>()))
                .Returns(Task.CompletedTask);

            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            _mockMapper
                .Setup(m => m.Map<ProductDetailsDto>(It.IsAny<Product>()))
                .Returns(expectedDto);

            _mockRecommendationService
                .Setup(s => s.GenerateAndSaveEmbeddingAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _productService.CreateAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.NameEn.Should().Be(dto.NameEn);
            result.Price.Should().Be(dto.Price);
            _mockImageValidationService.Verify(s => s.ValidateUrlsAsync(It.IsAny<IEnumerable<string>>()), Times.Once);
            _mockProductRepository.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WithImageValidationFailure_ThrowsException()
        {
            // Arrange
            var dto = new ProductCreateUpdateDto
            {
                NameEn = "New Chair",
                DescriptionEn = "A new product",
                Price = 150,
                StockQuantity = 20,
                ImageUrls = new List<string> { "https://invalid-url.com/image.jpg" }
            };

            var validationSummary = new ImageValidationSummary
            {
                AllApproved = false,
                Results = new List<ImageUrlValidationResult>()
            };

            _mockImageValidationService
                .Setup(s => s.ValidateUrlsAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(validationSummary);

            // Act & Assert
            await Assert.ThrowsAsync<ImageValidationException>(
                () => _productService.CreateAsync(dto)
            );

            _mockProductRepository.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_WithTooManyImages_ThrowsException()
        {
            // Arrange
            var imageUrls = Enumerable.Range(1, 6)
                .Select(i => $"https://example.com/image{i}.jpg")
                .ToList();

            var dto = new ProductCreateUpdateDto
            {
                NameEn = "New Chair",
                DescriptionEn = "A new product",
                Price = 150,
                StockQuantity = 20,
                CategoryId = 1,
                SellerId = "seller-1",
                ImageUrls = imageUrls
            };

            var validationSummary = new ImageValidationSummary
            {
                AllApproved = true,
                Results = new List<ImageUrlValidationResult>()
            };

            var product = new Product
            {
                Images = new List<ProductImage>()
            };

            _mockImageValidationService
                .Setup(s => s.ValidateUrlsAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(validationSummary);

            _mockMapper
                .Setup(m => m.Map<Product>(dto))
                .Returns(product);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _productService.CreateAsync(dto)
            );

            exception.Message.Should().Contain("at most");
        }

        [Fact]
        public async Task CreateAsync_WithoutImages_CreatesProduct()
        {
            // Arrange
            var dto = new ProductCreateUpdateDto
            {
                NameEn = "New Chair",
                DescriptionEn = "A new product",
                Price = 150,
                StockQuantity = 20,
                CategoryId = 1,
                SellerId = "seller-1",
                ImageUrls = new List<string>()
            };

            var product = new Product
            {
                Id = 1,
                NameEn = dto.NameEn,
                DescriptionEn = dto.DescriptionEn,
                Price = dto.Price,
                StockQuantity = dto.StockQuantity,
                CategoryId = dto.CategoryId,
                SellerId = dto.SellerId,
                CreatedAt = DateTime.UtcNow,
                Images = new List<ProductImage>()
            };

            var expectedDto = new ProductDetailsDto
            {
                Id = 1,
                NameEn = dto.NameEn,
                Name = dto.NameEn,
                Price = dto.Price
            };

            _mockMapper
                .Setup(m => m.Map<Product>(dto))
                .Returns(product);

            _mockProductRepository
                .Setup(r => r.AddAsync(It.IsAny<Product>()))
                .Returns(Task.CompletedTask);

            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            _mockMapper
                .Setup(m => m.Map<ProductDetailsDto>(It.IsAny<Product>()))
                .Returns(expectedDto);

            // Act
            var result = await _productService.CreateAsync(dto);

            // Assert
            result.Should().NotBeNull();
            _mockProductRepository.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Once);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_WithValidIdAndDto_UpdatesProduct()
        {
            // Arrange
            var productId = 1;
            var dto = new ProductCreateUpdateDto
            {
                NameEn = "Updated Chair",
                DescriptionEn = "Updated description",
                Price = 200,
                StockQuantity = 30,
                ImageUrls = new List<string> { "https://example.com/image1.jpg" }
            };

            var validationSummary = new ImageValidationSummary
            {
                AllApproved = true,
                Results = new List<ImageUrlValidationResult>()
            };

            var product = new Product
            {
                Id = productId,
                NameEn = "Old Chair",
                DescriptionEn = "Old description",
                Price = 100,
                StockQuantity = 10,
                Images = new List<ProductImage>()
            };

            _mockImageValidationService
                .Setup(s => s.ValidateUrlsAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(validationSummary);

            _mockProductRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<ISpecifications<Product, int>>()))
                .ReturnsAsync(product);

            _mockMapper
                .Setup(m => m.Map(dto, product))
                .Callback<ProductCreateUpdateDto, Product>((src, dest) =>
                {
                    dest.NameEn = src.NameEn;
                    dest.DescriptionEn = src.DescriptionEn;
                    dest.Price = src.Price;
                    dest.StockQuantity = src.StockQuantity;
                });

            _mockProductRepository
                .Setup(r => r.Update(It.IsAny<Product>()))
                .Callback<Product>(p => { }); // No-op callback

            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            _mockRecommendationService
                .Setup(s => s.GenerateAndSaveEmbeddingAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            await _productService.UpdateAsync(productId, dto);

            // Assert
            _mockProductRepository.Verify(r => r.GetByIdAsync(It.IsAny<ISpecifications<Product, int>>()), Times.Once);
            _mockProductRepository.Verify(r => r.Update(It.IsAny<Product>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WithInvalidId_ThrowsException()
        {
            // Arrange
            var productId = 999;
            var dto = new ProductCreateUpdateDto
            {
                NameEn = "Updated Chair",
                DescriptionEn = "Updated description",
                Price = 200,
                StockQuantity = 30,
                ImageUrls = new List<string>()
            };

            var validationSummary = new ImageValidationSummary
            {
                AllApproved = true,
                Results = new List<ImageUrlValidationResult>()
            };

            _mockImageValidationService
                .Setup(s => s.ValidateUrlsAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(validationSummary);

            _mockProductRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<ISpecifications<Product, int>>()))
                .ReturnsAsync((Product?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _productService.UpdateAsync(productId, dto)
            );

            exception.Message.Should().Contain("not found");
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_WithValidId_DeletesProduct()
        {
            // Arrange
            var productId = 1;
            var product = new Product { Id = productId };

            _mockProductRepository
                .Setup(r => r.GetByIdAsync(productId))
                .ReturnsAsync(product);

            _mockRecommendationService
                .Setup(s => s.DeleteProductEmbeddingAsync(productId))
                .Returns(Task.CompletedTask);

            _mockProductRepository
                .Setup(r => r.Remove(It.IsAny<Product>()))
                .Callback<Product>(p => { }); // No-op callback

            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _productService.DeleteAsync(productId);

            // Assert
            _mockProductRepository.Verify(r => r.GetByIdAsync(productId), Times.Once);
            _mockRecommendationService.Verify(s => s.DeleteProductEmbeddingAsync(productId), Times.Once);
            _mockProductRepository.Verify(r => r.Remove(It.IsAny<Product>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WithInvalidId_ThrowsException()
        {
            // Arrange
            var productId = 999;

            _mockProductRepository
                .Setup(r => r.GetByIdAsync(productId))
                .ReturnsAsync((Product?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _productService.DeleteAsync(productId)
            );

            exception.Message.Should().Contain("not found");
        }

        #endregion
    }
}

