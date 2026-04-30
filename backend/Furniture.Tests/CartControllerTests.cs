using Furniture.presentation.Controllers;
using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos;
using Furniture.shared.Dtos.Cart;
using Microsoft.AspNetCore.Mvc;
using Moq;
using FluentAssertions;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Furniture.Tests
{
    public class CartControllerTests
    {
        private readonly Mock<ICartService> _mockCartService;
        private readonly CartController _controller;

        public CartControllerTests()
        {
            _mockCartService = new Mock<ICartService>();
            _controller = new CartController(_mockCartService.Object);
        }

        private void SetupControllerContext(string? userId = null)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId ?? "test-user"),
                new Claim(ClaimTypes.Role, "buyer")
            };

            var identity = new ClaimsIdentity(claims, "TestScheme");
            var principal = new ClaimsPrincipal(identity);

            var httpContext = new DefaultHttpContext { User = principal };
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        #region GetCart Tests

        [Fact]
        public async Task GetCart_WithValidUserId_ReturnsOkWithCart()
        {
            // Arrange
            SetupControllerContext(userId: "user-1");
            var cartDto = new CartDto
            {
                Id = 1,
                CreatedAt = DateTime.UtcNow,
                TotalPrice = 300,
                TotalItems = 3,
                Items = new List<CartItemDto>
                {
                    new CartItemDto
                    {
                        ProductId = 1,
                        ProductName = "Chair",
                        UnitPrice = 100,
                        Quantity = 2,
                        SubTotal = 200,
                        AvailableStock = 10
                    },
                    new CartItemDto
                    {
                        ProductId = 2,
                        ProductName = "Table",
                        UnitPrice = 100,
                        Quantity = 1,
                        SubTotal = 100,
                        AvailableStock = 5
                    }
                }
            };

            _mockCartService
                .Setup(s => s.GetCartAsync("user-1"))
                .ReturnsAsync(cartDto);

            // Act
            var result = await _controller.GetCart();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().Be(cartDto);
            _mockCartService.Verify(s => s.GetCartAsync("user-1"), Times.Once);
        }

        [Fact]
        public async Task GetCart_WithEmptyCart_ReturnsEmptyCartDto()
        {
            // Arrange
            SetupControllerContext(userId: "user-1");
            var cartDto = new CartDto
            {
                Id = 1,
                CreatedAt = DateTime.UtcNow,
                TotalPrice = 0,
                TotalItems = 0,
                Items = new List<CartItemDto>()
            };

            _mockCartService
                .Setup(s => s.GetCartAsync("user-1"))
                .ReturnsAsync(cartDto);

            // Act
            var result = await _controller.GetCart();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var returnedCart = okResult!.Value as CartDto;
            returnedCart!.TotalItems.Should().Be(0);
            returnedCart.Items.Should().BeEmpty();
        }

        #endregion

        #region AddToCart Tests

        [Fact]
        public async Task AddToCart_WithValidDto_ReturnsOkWithUpdatedCart()
        {
            // Arrange
            SetupControllerContext(userId: "user-1");
            var addToCartDto = new AddToCartDto { ProductId = 1, Quantity = 2 };
            var updatedCartDto = new CartDto
            {
                Id = 1,
                CreatedAt = DateTime.UtcNow,
                TotalPrice = 200,
                TotalItems = 2,
                Items = new List<CartItemDto>
                {
                    new CartItemDto
                    {
                        ProductId = 1,
                        ProductName = "Chair",
                        UnitPrice = 100,
                        Quantity = 2,
                        SubTotal = 200,
                        AvailableStock = 10
                    }
                }
            };

            _mockCartService
                .Setup(s => s.AddToCartAsync("user-1", addToCartDto))
                .ReturnsAsync(updatedCartDto);

            // Act
            var result = await _controller.AddToCart(addToCartDto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().Be(updatedCartDto);
            _mockCartService.Verify(s => s.AddToCartAsync("user-1", addToCartDto), Times.Once);
        }

        [Fact]
        public async Task AddToCart_WithProductNotFound_ReturnsNotFound()
        {
            // Arrange
            SetupControllerContext(userId: "user-1");
            var addToCartDto = new AddToCartDto { ProductId = 999, Quantity = 1 };

            _mockCartService
                .Setup(s => s.AddToCartAsync("user-1", addToCartDto))
                .ThrowsAsync(new KeyNotFoundException("Product Not Found"));

            // Act
            var result = await _controller.AddToCart(addToCartDto);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult!.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task AddToCart_WithInsufficientStock_ReturnsBadRequest()
        {
            // Arrange
            SetupControllerContext(userId: "user-1");
            var addToCartDto = new AddToCartDto { ProductId = 1, Quantity = 20 };

            _mockCartService
                .Setup(s => s.AddToCartAsync("user-1", addToCartDto))
                .ThrowsAsync(new InvalidOperationException("This Quantity Is Not Available . The Available Is: 10"));

            // Act
            var result = await _controller.AddToCart(addToCartDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task AddToCart_WithInvalidQuantity_ReturnsBadRequest()
        {
            // Arrange
            SetupControllerContext(userId: "user-1");
            var addToCartDto = new AddToCartDto { ProductId = 1, Quantity = 0 };

            _mockCartService
                .Setup(s => s.AddToCartAsync("user-1", addToCartDto))
                .ThrowsAsync(new InvalidOperationException("Quantity Must Be Over 0"));

            // Act
            var result = await _controller.AddToCart(addToCartDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        #endregion

        #region UpdateCartItem Tests

        [Fact]
        public async Task UpdateCartItem_WithValidProductIdAndQuantity_ReturnsOkWithUpdatedCart()
        {
            // Arrange
            SetupControllerContext(userId: "user-1");
            var productId = 1;
            var updateDto = new UpdateCartItemDto { Quantity = 5 };
            var updatedCartDto = new CartDto
            {
                Id = 1,
                CreatedAt = DateTime.UtcNow,
                TotalPrice = 500,
                TotalItems = 5,
                Items = new List<CartItemDto>
                {
                    new CartItemDto
                    {
                        ProductId = 1,
                        ProductName = "Chair",
                        UnitPrice = 100,
                        Quantity = 5,
                        SubTotal = 500,
                        AvailableStock = 10
                    }
                }
            };

            _mockCartService
                .Setup(s => s.UpdateCartItemAsync("user-1", productId, updateDto))
                .ReturnsAsync(updatedCartDto);

            // Act
            var result = await _controller.UpdateCartItem(productId, updateDto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().Be(updatedCartDto);
            _mockCartService.Verify(s => s.UpdateCartItemAsync("user-1", productId, updateDto), Times.Once);
        }

        [Fact]
        public async Task UpdateCartItem_WithProductNotInCart_ReturnsNotFound()
        {
            // Arrange
            SetupControllerContext(userId: "user-1");
            var productId = 999;
            var updateDto = new UpdateCartItemDto { Quantity = 5 };

            _mockCartService
                .Setup(s => s.UpdateCartItemAsync("user-1", productId, updateDto))
                .ThrowsAsync(new KeyNotFoundException("Product Is Not In The Cart !"));

            // Act
            var result = await _controller.UpdateCartItem(productId, updateDto);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task UpdateCartItem_WithInsufficientStock_ReturnsBadRequest()
        {
            // Arrange
            SetupControllerContext(userId: "user-1");
            var productId = 1;
            var updateDto = new UpdateCartItemDto { Quantity = 20 };

            _mockCartService
                .Setup(s => s.UpdateCartItemAsync("user-1", productId, updateDto))
                .ThrowsAsync(new InvalidOperationException("This Quantity Is Not Available . The Available Is: 10"));

            // Act
            var result = await _controller.UpdateCartItem(productId, updateDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task UpdateCartItem_WithInvalidQuantity_ReturnsBadRequest()
        {
            // Arrange
            SetupControllerContext(userId: "user-1");
            var productId = 1;
            var updateDto = new UpdateCartItemDto { Quantity = 0 };

            _mockCartService
                .Setup(s => s.UpdateCartItemAsync("user-1", productId, updateDto))
                .ThrowsAsync(new InvalidOperationException("Quantity Must Be Over 0"));

            // Act
            var result = await _controller.UpdateCartItem(productId, updateDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        #endregion

        #region RemoveFromCart Tests

        [Fact]
        public async Task RemoveFromCart_WithValidProductId_ReturnsOkWithUpdatedCart()
        {
            // Arrange
            SetupControllerContext(userId: "user-1");
            var productId = 1;
            var updatedCartDto = new CartDto
            {
                Id = 1,
                CreatedAt = DateTime.UtcNow,
                TotalPrice = 0,
                TotalItems = 0,
                Items = new List<CartItemDto>()
            };

            _mockCartService
                .Setup(s => s.RemoveFromCartAsync("user-1", productId))
                .ReturnsAsync(updatedCartDto);

            // Act
            var result = await _controller.RemoveFromCart(productId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().Be(updatedCartDto);
            _mockCartService.Verify(s => s.RemoveFromCartAsync("user-1", productId), Times.Once);
        }

        [Fact]
        public async Task RemoveFromCart_WithProductNotInCart_ReturnsNotFound()
        {
            // Arrange
            SetupControllerContext(userId: "user-1");
            var productId = 999;

            _mockCartService
                .Setup(s => s.RemoveFromCartAsync("user-1", productId))
                .ThrowsAsync(new KeyNotFoundException("Product Is Not In The Cart !"));

            // Act
            var result = await _controller.RemoveFromCart(productId);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task RemoveFromCart_WithEmptyCart_ReturnsNotFound()
        {
            // Arrange
            SetupControllerContext(userId: "user-1");
            var productId = 1;

            _mockCartService
                .Setup(s => s.RemoveFromCartAsync("user-1", productId))
                .ThrowsAsync(new KeyNotFoundException("Product Is Not In The Cart !"));

            // Act
            var result = await _controller.RemoveFromCart(productId);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        #endregion

        #region ClearCart Tests

        [Fact]
        public async Task ClearCart_WithCartItems_ReturnsOkWithSuccessMessage()
        {
            // Arrange
            SetupControllerContext(userId: "user-1");

            _mockCartService
                .Setup(s => s.ClearCartAsync("user-1"))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.ClearCart();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().NotBeNull();
            _mockCartService.Verify(s => s.ClearCartAsync("user-1"), Times.Once);
        }

        [Fact]
        public async Task ClearCart_WithEmptyCart_ReturnsOk()
        {
            // Arrange
            SetupControllerContext(userId: "user-1");

            _mockCartService
                .Setup(s => s.ClearCartAsync("user-1"))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.ClearCart();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        #endregion
    }
}

