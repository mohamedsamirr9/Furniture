using AutoMapper;
using Furniture.Domain.InterfacesRepositories;
using Furniture.Domain.Models;
using Furniture.Services;
using Furniture.Services.Specifications;
using Furniture.shared.Dtos.CategoryDto;
using Furniture.shared.Dtos.ProductDtos;
using Moq;
using FluentAssertions;

namespace Furniture.Tests
{
    public class CategoryServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IGenaricRepository<Category, int>> _mockCategoryRepository;
        private readonly CategoryService _categoryService;

        public CategoryServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockCategoryRepository = new Mock<IGenaricRepository<Category, int>>();

            _mockUnitOfWork
                .Setup(u => u.GetRepository<Category, int>())
                .Returns(_mockCategoryRepository.Object);

            _categoryService = new CategoryService(_mockUnitOfWork.Object, _mockMapper.Object);
        }

        #region GetAllCategoriesAsync Tests

        [Fact]
        public async Task GetAllCategoriesAsync_WithValidParams_ReturnsCategories()
        {
            // Arrange
            var pageIndex = 0;
            var pageSize = 10;
            var search = "furniture";
            var language = "en";

            var categories = new List<Category>
            {
                new Category
                {
                    Id = 1,
                    NameEn = "Furniture",
                    NameAr = "أثاث",
                    DescriptionEn = "Furniture items",
                    DescriptionAr = "عناصر أثاث"
                }
            };

            var categoryDtos = new List<CategoryListDto>
            {
                new CategoryListDto
                {
                    Id = 1,
                    Name = "Furniture",
                    NameEn = "Furniture",
                    NameAr = "أثاث"
                }
            };

            _mockCategoryRepository
                .Setup(r => r.GetAllAsync(It.IsAny<ISpecifications<Category, int>>()))
                .ReturnsAsync(categories);

            _mockMapper
                .Setup(m => m.Map<List<CategoryListDto>>(categories))
                .Returns(categoryDtos);

            // Act
            var result = await _categoryService.GetAllCategoriesAsync(pageIndex, pageSize, search, language);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.First().NameEn.Should().Be("Furniture");
            _mockCategoryRepository.Verify(
                r => r.GetAllAsync(It.IsAny<ISpecifications<Category, int>>()),
                Times.Once
            );
        }

        [Fact]
        public async Task GetAllCategoriesAsync_WithEmptySearch_ReturnsAllCategories()
        {
            // Arrange
            var pageIndex = 0;
            var pageSize = 10;
            string? search = null;
            var language = "en";

            var categories = new List<Category>
            {
                new Category { Id = 1, NameEn = "Furniture", NameAr = "أثاث" },
                new Category { Id = 2, NameEn = "Decor", NameAr = "ديكور" }
            };

            var categoryDtos = new List<CategoryListDto>
            {
                new CategoryListDto { Id = 1, Name = "Furniture", NameEn = "Furniture" },
                new CategoryListDto { Id = 2, Name = "Decor", NameEn = "Decor" }
            };

            _mockCategoryRepository
                .Setup(r => r.GetAllAsync(It.IsAny<ISpecifications<Category, int>>()))
                .ReturnsAsync(categories);

            _mockMapper
                .Setup(m => m.Map<List<CategoryListDto>>(categories))
                .Returns(categoryDtos);

            // Act
            var result = await _categoryService.GetAllCategoriesAsync(pageIndex, pageSize, search, language);

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllCategoriesAsync_WithArabicLanguage_LocalizesToArabic()
        {
            // Arrange
            var pageIndex = 0;
            var pageSize = 10;
            string? search = null;
            var language = "ar";

            var categories = new List<Category>
            {
                new Category
                {
                    Id = 1,
                    NameEn = "Furniture",
                    NameAr = "أثاث",
                    DescriptionEn = "Furniture items",
                    DescriptionAr = "عناصر أثاث"
                }
            };

            var categoryDtos = new List<CategoryListDto>
            {
                new CategoryListDto
                {
                    Id = 1,
                    Name = "أثاث",
                    NameEn = "Furniture",
                    NameAr = "أثاث"
                }
            };

            _mockCategoryRepository
                .Setup(r => r.GetAllAsync(It.IsAny<ISpecifications<Category, int>>()))
                .ReturnsAsync(categories);

            _mockMapper
                .Setup(m => m.Map<List<CategoryListDto>>(categories))
                .Returns(categoryDtos);

            // Act
            var result = await _categoryService.GetAllCategoriesAsync(pageIndex, pageSize, search, language);

            // Assert
            result.Should().NotBeNull();
            result.First().Name.Should().Be("أثاث");
        }

        #endregion

        #region GetCategoryByIdAsync Tests

        [Fact]
        public async Task GetCategoryByIdAsync_WithValidId_ReturnsCategory()
        {
            // Arrange
            var categoryId = 1;
            var language = "en";

            var category = new Category
            {
                Id = categoryId,
                NameEn = "Furniture",
                NameAr = "أثاث",
                DescriptionEn = "Furniture items",
                DescriptionAr = "عناصر أثاث",
                Products = new List<Product>()
            };

            var expectedDto = new CategoryDto
            {
                Id = categoryId,
                Name = "Furniture",
                NameEn = "Furniture",
                NameAr = "أثاث",
                Description = "Furniture items",
                Products = new List<ProductListDto>()
            };

            _mockCategoryRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<ISpecifications<Category, int>>()))
                .ReturnsAsync(category);

            _mockMapper
                .Setup(m => m.Map<CategoryDto>(category))
                .Returns(expectedDto);

            // Act
            var result = await _categoryService.GetCategoryByIdAsync(categoryId, language);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(categoryId);
            result.NameEn.Should().Be("Furniture");
            _mockCategoryRepository.Verify(
                r => r.GetByIdAsync(It.IsAny<ISpecifications<Category, int>>()),
                Times.Once
            );
        }

        [Fact]
        public async Task GetCategoryByIdAsync_WithInvalidId_ThrowsException()
        {
            // Arrange
            var categoryId = 999;
            var language = "en";

            _mockCategoryRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<ISpecifications<Category, int>>()))
                .ReturnsAsync((Category?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _categoryService.GetCategoryByIdAsync(categoryId, language)
            );

            exception.Message.Should().Contain("not found");
        }

        [Fact]
        public async Task GetCategoryByIdAsync_WithProducts_ReturnsProductsList()
        {
            // Arrange
            var categoryId = 1;
            var language = "en";

            var products = new List<Product>
            {
                new Product
                {
                    Id = 1,
                    NameEn = "Chair",
                    NameAr = "كرسي",
                    Price = 100,
                    CategoryId = categoryId
                },
                new Product
                {
                    Id = 2,
                    NameEn = "Table",
                    NameAr = "طاولة",
                    Price = 200,
                    CategoryId = categoryId
                }
            };

            var category = new Category
            {
                Id = categoryId,
                NameEn = "Furniture",
                NameAr = "أثاث",
                DescriptionEn = "Furniture items",
                Products = products
            };

            var productDtos = new List<ProductListDto>
            {
                new ProductListDto { Id = 1, Name = "Chair", Price = 100 },
                new ProductListDto { Id = 2, Name = "Table", Price = 200 }
            };

            var expectedDto = new CategoryDto
            {
                Id = categoryId,
                Name = "Furniture",
                NameEn = "Furniture",
                Description = "Furniture items",
                Products = productDtos
            };

            _mockCategoryRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<ISpecifications<Category, int>>()))
                .ReturnsAsync(category);

            _mockMapper
                .Setup(m => m.Map<CategoryDto>(category))
                .Returns(expectedDto);

            // Act
            var result = await _categoryService.GetCategoryByIdAsync(categoryId, language);

            // Assert
            result.Should().NotBeNull();
            result.Products.Should().HaveCount(2);
        }

        #endregion

        #region CreateCategoryAsync Tests

        [Fact]
        public async Task CreateCategoryAsync_WithValidDto_CreatesCategory()
        {
            // Arrange
            var dto = new CategoryCreateUpdateDto
            {
                NameEn = "New Category",
                NameAr = "فئة جديدة",
                DescriptionEn = "New category description",
                DescriptionAr = "وصف الفئة الجديدة",
                Image = "https://example.com/image.jpg"
            };

            var category = new Category
            {
                Id = 1,
                NameEn = dto.NameEn,
                NameAr = dto.NameAr,
                DescriptionEn = dto.DescriptionEn,
                DescriptionAr = dto.DescriptionAr,
                Image = dto.Image,
                Created_At = DateTime.UtcNow
            };

            var expectedDto = new CategoryDto
            {
                Id = 1,
                Name = dto.NameEn,
                NameEn = dto.NameEn,
                NameAr = dto.NameAr,
                Description = dto.DescriptionEn,
                DescriptionEn = dto.DescriptionEn,
                DescriptionAr = dto.DescriptionAr,
                Image = dto.Image
            };

            _mockMapper
                .Setup(m => m.Map<Category>(dto))
                .Returns(category);

            _mockCategoryRepository
                .Setup(r => r.AddAsync(It.IsAny<Category>()))
                .Returns(Task.CompletedTask);

            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            _mockMapper
                .Setup(m => m.Map<CategoryDto>(category))
                .Returns(expectedDto);

            // Act
            var result = await _categoryService.CreateCategoryAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.NameEn.Should().Be(dto.NameEn);
            result.DescriptionEn.Should().Be(dto.DescriptionEn);
            _mockCategoryRepository.Verify(r => r.AddAsync(It.IsAny<Category>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        #endregion

        #region UpdateCategoryAsync Tests

        [Fact]
        public async Task UpdateCategoryAsync_WithValidIdAndDto_UpdatesCategory()
        {
            // Arrange
            var categoryId = 1;
            var dto = new CategoryCreateUpdateDto
            {
                NameEn = "Updated Category",
                NameAr = "فئة محدثة",
                DescriptionEn = "Updated description",
                DescriptionAr = "وصف محدث"
            };

            var existingCategory = new Category
            {
                Id = categoryId,
                NameEn = "Old Category",
                NameAr = "فئة قديمة",
                DescriptionEn = "Old description"
            };

            _mockCategoryRepository
                .Setup(r => r.GetByIdAsync(categoryId))
                .ReturnsAsync(existingCategory);

            _mockMapper
                .Setup(m => m.Map(dto, existingCategory))
                .Callback<CategoryCreateUpdateDto, Category>((src, dest) =>
                {
                    dest.NameEn = src.NameEn;
                    dest.NameAr = src.NameAr;
                    dest.DescriptionEn = src.DescriptionEn;
                    dest.DescriptionAr = src.DescriptionAr;
                });

            _mockCategoryRepository
                .Setup(r => r.Update(It.IsAny<Category>()))
                .Callback<Category>(c => { }); // No-op callback

            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _categoryService.UpdateCategoryAsync(categoryId, dto);

            // Assert
            _mockCategoryRepository.Verify(r => r.GetByIdAsync(categoryId), Times.Once);
            _mockCategoryRepository.Verify(r => r.Update(It.IsAny<Category>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateCategoryAsync_WithInvalidId_ThrowsException()
        {
            // Arrange
            var categoryId = 999;
            var dto = new CategoryCreateUpdateDto
            {
                NameEn = "Updated Category",
                DescriptionEn = "Updated description"
            };

            _mockCategoryRepository
                .Setup(r => r.GetByIdAsync(categoryId))
                .ReturnsAsync((Category?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _categoryService.UpdateCategoryAsync(categoryId, dto)
            );

            exception.Message.Should().Contain("not found");
        }

        #endregion

        #region DeleteCategoryAsync Tests

        [Fact]
        public async Task DeleteCategoryAsync_WithValidId_DeletesCategory()
        {
            // Arrange
            var categoryId = 1;
            var category = new Category { Id = categoryId, NameEn = "Furniture" };

            _mockCategoryRepository
                .Setup(r => r.GetByIdAsync(categoryId))
                .ReturnsAsync(category);

            _mockCategoryRepository
                .Setup(r => r.Remove(It.IsAny<Category>()))
                .Callback<Category>(c => { }); // No-op callback

            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _categoryService.DeleteCategoryAsync(categoryId);

            // Assert
            _mockCategoryRepository.Verify(r => r.GetByIdAsync(categoryId), Times.Once);
            _mockCategoryRepository.Verify(r => r.Remove(It.IsAny<Category>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteCategoryAsync_WithInvalidId_DoesNotThrow()
        {
            // Arrange
            var categoryId = 999;

            _mockCategoryRepository
                .Setup(r => r.GetByIdAsync(categoryId))
                .ReturnsAsync((Category?)null);

            // Act - Should not throw
            await _categoryService.DeleteCategoryAsync(categoryId);

            // Assert
            _mockCategoryRepository.Verify(r => r.Remove(It.IsAny<Category>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        #endregion
    }
}

