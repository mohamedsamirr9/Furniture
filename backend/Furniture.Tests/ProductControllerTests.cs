using Furniture.presentation.Controllers;
using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos.ProductDtos;
using Microsoft.AspNetCore.Mvc;
using Moq;
using FluentAssertions;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Furniture.Tests
{
    public class ProductControllerTests
    {
        private readonly Mock<IProductService> _mockProductService;
        private readonly ProductController _controller;

        public ProductControllerTests()
        {
            _mockProductService = new Mock<IProductService>();
            _controller = new ProductController(_mockProductService.Object);
        }

        private void SetupControllerContext(string? userId = null, string role = "buyer")
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId ?? "test-user")
            };

            if (!string.IsNullOrEmpty(role))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var identity = new ClaimsIdentity(claims, "TestScheme");
            var principal = new ClaimsPrincipal(identity);

            var httpContext = new DefaultHttpContext { User = principal };
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        #region GetAll Tests

        [Fact]
        public async Task GetAll_WithValidQueryParams_ReturnsOkWithPaginatedProducts()
        {
            // Arrange
            SetupControllerContext();
            var queryParams = new ProductQueryParams { Page = 1, PageSize = 10 };
            var paginatedDto = new PaginatedProductsDto
            {
                TotalCount = 2,
                Page = 1,
                PageSize = 10,
                Data = new List<ProductListDto>
                {
                    new ProductListDto
                    {
                        Id = 1,
                        Name = "Chair",
                        Price = 100,
                        CategoryName = "Furniture"
                    },
                    new ProductListDto
                    {
                        Id = 2,
                        Name = "Table",
                        Price = 200,
                        CategoryName = "Furniture"
                    }
                }
            };

            _mockProductService
                .Setup(s => s.GetAllAsync(It.IsAny<ProductQueryParams>(), It.IsAny<string>()))
                .ReturnsAsync(paginatedDto);

            // Act
            var result = await _controller.GetAll(queryParams);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().Be(paginatedDto);
            _mockProductService.Verify(
                s => s.GetAllAsync(It.IsAny<ProductQueryParams>(), It.IsAny<string>()),
                Times.Once
            );
        }

        [Fact]
        public async Task GetAll_WithEmptyResult_ReturnsOkWithEmptyData()
        {
            // Arrange
            SetupControllerContext();
            var queryParams = new ProductQueryParams { Page = 1, PageSize = 10 };
            var paginatedDto = new PaginatedProductsDto
            {
                TotalCount = 0,
                Page = 1,
                PageSize = 10,
                Data = new List<ProductListDto>()
            };

            _mockProductService
                .Setup(s => s.GetAllAsync(It.IsAny<ProductQueryParams>(), It.IsAny<string>()))
                .ReturnsAsync(paginatedDto);

            // Act
            var result = await _controller.GetAll(queryParams);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var returnedDto = okResult!.Value as PaginatedProductsDto;
            returnedDto!.Data.Should().BeEmpty();
        }

        #endregion

        #region GetById Tests

        [Fact]
        public async Task GetById_WithValidId_ReturnsOkWithProduct()
        {
            // Arrange
            SetupControllerContext();
            var productId = 1;
            var productDetailsDto = new ProductDetailsDto
            {
                Id = productId,
                Name = "Chair",
                Price = 100,
                StockQuantity = 10,
                CategoryName = "Furniture"
            };

            _mockProductService
                .Setup(s => s.GetByIdAsync(productId, It.IsAny<string>()))
                .ReturnsAsync(productDetailsDto);

            // Act
            var result = await _controller.GetById(productId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().Be(productDetailsDto);
        }

        [Fact]
        public async Task GetById_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            SetupControllerContext();
            var productId = 999;

            _mockProductService
                .Setup(s => s.GetByIdAsync(productId, It.IsAny<string>()))
                .ReturnsAsync((ProductDetailsDto?)null);

            // Act
            var result = await _controller.GetById(productId);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult!.Value.Should().NotBeNull();
        }

        #endregion

        #region GetSellerProducts Tests

        [Fact]
        public async Task GetSellerProducts_WithValidSeller_ReturnsOkWithSellerProducts()
        {
            // Arrange
            SetupControllerContext(userId: "seller-1", role: "seller");
            var queryParams = new ProductQueryParams { Page = 1, PageSize = 10 };
            var paginatedDto = new PaginatedProductsDto
            {
                TotalCount = 1,
                Page = 1,
                PageSize = 10,
                Data = new List<ProductListDto>
                {
                    new ProductListDto
                    {
                        Id = 1,
                        Name = "Seller's Chair",
                        Price = 100,
                        CategoryName = "Furniture"
                    }
                }
            };

            _mockProductService
                .Setup(s => s.GetSellerProductsAsync("seller-1", It.IsAny<ProductQueryParams>(), It.IsAny<string>()))
                .ReturnsAsync(paginatedDto);

            // Act
            var result = await _controller.GetSellerProducts(queryParams);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().Be(paginatedDto);
        }

        [Fact]
        public async Task GetSellerProducts_WithoutUserId_ReturnsUnauthorized()
        {
            // Arrange
            var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal() };
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            var queryParams = new ProductQueryParams();

            // Act
            var result = await _controller.GetSellerProducts(queryParams);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        #endregion

        #region GetSellerProductById Tests

        [Fact]
        public async Task GetSellerProductById_WithOwnProduct_ReturnsOk()
        {
            // Arrange
            SetupControllerContext(userId: "seller-1", role: "seller");
            var productId = 1;
            var productDetailsDto = new ProductDetailsDto
            {
                Id = productId,
                Name = "Chair",
                Price = 100,
                SellerId = "seller-1",
                CategoryName = "Furniture"
            };

            _mockProductService
                .Setup(s => s.GetByIdAsync(productId, It.IsAny<string>()))
                .ReturnsAsync(productDetailsDto);

            // Act
            var result = await _controller.GetSellerProductById(productId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetSellerProductById_WithOtherSellerProduct_ReturnsNotFound()
        {
            // Arrange
            SetupControllerContext(userId: "seller-1", role: "seller");
            var productId = 1;
            var productDetailsDto = new ProductDetailsDto
            {
                Id = productId,
                Name = "Chair",
                Price = 100,
                SellerId = "seller-2", // Different seller
                CategoryName = "Furniture"
            };

            _mockProductService
                .Setup(s => s.GetByIdAsync(productId, It.IsAny<string>()))
                .ReturnsAsync(productDetailsDto);

            // Act
            var result = await _controller.GetSellerProductById(productId);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetSellerProductById_WithoutUserId_ReturnsUnauthorized()
        {
            // Arrange
            var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal() };
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            var productId = 1;

            // Act
            var result = await _controller.GetSellerProductById(productId);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        #endregion

        #region Create Tests

        [Fact]
        public async Task Create_WithValidDto_ReturnsCreatedAtAction()
        {
            // Arrange
            SetupControllerContext(userId: "seller-1", role: "seller");
            var dto = new ProductCreateUpdateDto
            {
                NameEn = "New Chair",
                DescriptionEn = "A new product",
                Price = 150,
                StockQuantity = 20,
                IsAvailable = true,
                CategoryId = 1,
                SellerId = "seller-1",
                ImageUrls = new List<string> { "https://example.com/image.jpg" }
            };

            var createdDto = new ProductDetailsDto
            {
                Id = 1,
                Name = "New Chair",
                NameEn = "New Chair",
                Price = 150,
                StockQuantity = 20,
                CategoryId = 1
            };

            _mockProductService
                .Setup(s => s.CreateAsync(dto, It.IsAny<string>()))
                .ReturnsAsync(createdDto);

            // Act
            var result = await _controller.Create(dto);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result as CreatedAtActionResult;
            createdResult!.Value.Should().Be(createdDto);
        }

        [Fact]
        public async Task Create_WithTooManyImages_ReturnsBadRequest()
        {
            // Arrange
            SetupControllerContext(userId: "seller-1", role: "seller");
            var imageUrls = Enumerable.Range(1, 6)
                .Select(i => $"https://example.com/image{i}.jpg")
                .ToList();

            var dto = new ProductCreateUpdateDto
            {
                NameEn = "New Chair",
                DescriptionEn = "A new product",
                Price = 150,
                StockQuantity = 20,
                ImageUrls = imageUrls
            };

            // Act
            var result = await _controller.Create(dto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        #endregion

        #region Update Tests

        [Fact]
        public async Task Update_WithValidIdAndDto_ReturnsNoContent()
        {
            // Arrange
            SetupControllerContext(userId: "seller-1", role: "seller");
            var productId = 1;
            var dto = new ProductCreateUpdateDto
            {
                NameEn = "Updated Chair",
                DescriptionEn = "Updated description",
                Price = 200,
                StockQuantity = 30,
                ImageUrls = new List<string>()
            };

            _mockProductService
                .Setup(s => s.UpdateAsync(productId, dto))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Update(productId, dto);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task Update_WithInvalidId_ThrowsException()
        {
            // Arrange
            SetupControllerContext(userId: "seller-1", role: "seller");
            var productId = 999;
            var dto = new ProductCreateUpdateDto
            {
                NameEn = "Updated Chair",
                DescriptionEn = "Updated description",
                Price = 200,
                StockQuantity = 30,
                ImageUrls = new List<string>()
            };

            _mockProductService
                .Setup(s => s.UpdateAsync(productId, dto))
                .ThrowsAsync(new Exception($"Product with id {productId} not found"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(
                () => _controller.Update(productId, dto)
            );
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task Delete_WithValidId_ReturnsNoContent()
        {
            // Arrange
            SetupControllerContext(userId: "seller-1", role: "seller");
            var productId = 1;

            _mockProductService
                .Setup(s => s.DeleteAsync(productId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Delete(productId);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task Delete_WithInvalidId_ThrowsException()
        {
            // Arrange
            SetupControllerContext(userId: "seller-1", role: "seller");
            var productId = 999;

            _mockProductService
                .Setup(s => s.DeleteAsync(productId))
                .ThrowsAsync(new Exception($"Product with id {productId} not found"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(
                () => _controller.Delete(productId)
            );
        }

        #endregion
    }
}

