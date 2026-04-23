using AutoMapper;
using Furniture.Domain.InterfacesRepositories;
using Furniture.Domain.Models;
using Furniture.Services;
using Furniture.Services.Specifications;
using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos;
using Furniture.shared.Dtos.Cart;
using Moq;
using FluentAssertions;

namespace Furniture.Tests
{
    public class CartServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IRecommendationService> _mockRecommendationService;
        private readonly Mock<IGenaricRepository<Cart, int>> _mockCartRepository;
        private readonly Mock<IGenaricRepository<Product, int>> _mockProductRepository;
        private readonly Mock<IGenaricRepository<CartItem, int>> _mockCartItemRepository;
        private readonly CartService _cartService;

        public CartServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockRecommendationService = new Mock<IRecommendationService>();
            _mockCartRepository = new Mock<IGenaricRepository<Cart, int>>();
            _mockProductRepository = new Mock<IGenaricRepository<Product, int>>();
            _mockCartItemRepository = new Mock<IGenaricRepository<CartItem, int>>();

            _mockUnitOfWork
                .Setup(u => u.GetRepository<Cart, int>())
                .Returns(_mockCartRepository.Object);

            _mockUnitOfWork
                .Setup(u => u.GetRepository<Product, int>())
                .Returns(_mockProductRepository.Object);

            _mockUnitOfWork
                .Setup(u => u.GetRepository<CartItem, int>())
                .Returns(_mockCartItemRepository.Object);

            _cartService = new CartService(
                _mockUnitOfWork.Object,
                _mockMapper.Object,
                _mockRecommendationService.Object
            );
        }

        #region GetCartAsync Tests

        [Fact]
        public async Task GetCartAsync_WithExistingCart_ReturnsCartDto()
        {
            // Arrange
            var userId = "user-1";
            var cart = new Cart
            {
                Id = 1,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                CartItems = new List<CartItem>()
            };

            var expectedCartDto = new CartDto
            {
                Id = 1,
                CreatedAt = cart.CreatedAt,
                TotalPrice = 0,
                TotalItems = 0,
                Items = new List<CartItemDto>()
            };

            _mockCartRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<ISpecifications<Cart, int>>()))
                .ReturnsAsync(cart);

            _mockMapper
                .Setup(m => m.Map<CartDto>(cart))
                .Returns(expectedCartDto);

            // Act
            var result = await _cartService.GetCartAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.TotalPrice.Should().Be(0);
            result.TotalItems.Should().Be(0);
            _mockCartRepository.Verify(r => r.GetByIdAsync(It.IsAny<ISpecifications<Cart, int>>()), Times.Once);
        }

        [Fact]
        public async Task GetCartAsync_WithNonExistentCart_CreatesNewCart()
        {
            // Arrange
            var userId = "user-1";
            var newCart = new Cart
            {
                Id = 1,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                CartItems = new List<CartItem>()
            };

            var expectedCartDto = new CartDto
            {
                Id = 1,
                CreatedAt = newCart.CreatedAt,
                TotalPrice = 0,
                TotalItems = 0,
                Items = new List<CartItemDto>()
            };

            _mockCartRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<ISpecifications<Cart, int>>()))
                .ReturnsAsync((Cart?)null);

            _mockCartRepository
                .Setup(r => r.AddAsync(It.IsAny<Cart>()))
                .Returns(Task.CompletedTask);

            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            _mockMapper
                .Setup(m => m.Map<CartDto>(It.IsAny<Cart>()))
                .Returns(expectedCartDto);

            // Act
            var result = await _cartService.GetCartAsync(userId);

            // Assert
            result.Should().NotBeNull();
            _mockCartRepository.Verify(r => r.AddAsync(It.IsAny<Cart>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        #endregion

        #region AddToCartAsync Tests

        [Fact]
        public async Task AddToCartAsync_WithValidProduct_AddsItemToCart()
        {
            // Arrange
            var userId = "user-1";
            var addToCartDto = new AddToCartDto { ProductId = 1, Quantity = 2 };

            var product = new Product
            {
                Id = 1,
                NameEn = "Chair",
                Price = 100,
                StockQuantity = 10
            };

            var cart = new Cart
            {
                Id = 1,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                CartItems = new List<CartItem>()
            };

            var expectedCartDto = new CartDto
            {
                Id = 1,
                CreatedAt = cart.CreatedAt,
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

            _mockProductRepository
                .Setup(r => r.GetByIdAsync(addToCartDto.ProductId))
                .ReturnsAsync(product);

            _mockCartRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<ISpecifications<Cart, int>>()))
                .ReturnsAsync(cart);

            _mockCartItemRepository
                .Setup(r => r.AddAsync(It.IsAny<CartItem>()))
                .Returns(Task.CompletedTask);

            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            _mockRecommendationService
                .Setup(s => s.UpdateUserEmbeddingAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _mockMapper
                .Setup(m => m.Map<CartDto>(It.IsAny<Cart>()))
                .Returns(expectedCartDto);

            // Act
            var result = await _cartService.AddToCartAsync(userId, addToCartDto);

            // Assert
            result.Should().NotBeNull();
            result.TotalItems.Should().Be(2);
            result.TotalPrice.Should().Be(200);
            _mockCartItemRepository.Verify(r => r.AddAsync(It.IsAny<CartItem>()), Times.Once);
        }

        [Fact]
        public async Task AddToCartAsync_WithInvalidQuantity_ThrowsException()
        {
            // Arrange
            var userId = "user-1";
            var addToCartDto = new AddToCartDto { ProductId = 1, Quantity = 0 };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _cartService.AddToCartAsync(userId, addToCartDto)
            );
        }

        [Fact]
        public async Task AddToCartAsync_WithNonExistentProduct_ThrowsException()
        {
            // Arrange
            var userId = "user-1";
            var addToCartDto = new AddToCartDto { ProductId = 999, Quantity = 1 };

            _mockProductRepository
                .Setup(r => r.GetByIdAsync(addToCartDto.ProductId))
                .ReturnsAsync((Product?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _cartService.AddToCartAsync(userId, addToCartDto)
            );
        }

        [Fact]
        public async Task AddToCartAsync_WithInsufficientStock_ThrowsException()
        {
            // Arrange
            var userId = "user-1";
            var addToCartDto = new AddToCartDto { ProductId = 1, Quantity = 20 };

            var product = new Product
            {
                Id = 1,
                NameEn = "Chair",
                Price = 100,
                StockQuantity = 10
            };

            var cart = new Cart
            {
                Id = 1,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                CartItems = new List<CartItem>()
            };

            _mockProductRepository
                .Setup(r => r.GetByIdAsync(addToCartDto.ProductId))
                .ReturnsAsync(product);

            _mockCartRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<ISpecifications<Cart, int>>()))
                .ReturnsAsync(cart);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _cartService.AddToCartAsync(userId, addToCartDto)
            );

            exception.Message.Should().Contain("Not Available");
        }

        [Fact]
        public async Task AddToCartAsync_WithExistingItem_UpdatesQuantity()
        {
            // Arrange
            var userId = "user-1";
            var addToCartDto = new AddToCartDto { ProductId = 1, Quantity = 2 };

            var product = new Product
            {
                Id = 1,
                NameEn = "Chair",
                Price = 100,
                StockQuantity = 10
            };

            var existingItem = new CartItem
            {
                ProductId = 1,
                Quantity = 1,
                UnitPrice = 100
            };

            var cart = new Cart
            {
                Id = 1,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                CartItems = new List<CartItem> { existingItem }
            };

            var expectedCartDto = new CartDto
            {
                Id = 1,
                CreatedAt = cart.CreatedAt,
                TotalPrice = 300,
                TotalItems = 3,
                Items = new List<CartItemDto>()
            };

            _mockProductRepository
                .Setup(r => r.GetByIdAsync(addToCartDto.ProductId))
                .ReturnsAsync(product);

            _mockCartRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<ISpecifications<Cart, int>>()))
                .ReturnsAsync(cart);

            _mockCartItemRepository
                .Setup(r => r.Update(It.IsAny<CartItem>()))
                .Callback<CartItem>(ci => { }); // No-op callback

            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            _mockRecommendationService
                .Setup(s => s.UpdateUserEmbeddingAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _mockMapper
                .Setup(m => m.Map<CartDto>(It.IsAny<Cart>()))
                .Returns(expectedCartDto);

            // Act
            var result = await _cartService.AddToCartAsync(userId, addToCartDto);

            // Assert
            result.Should().NotBeNull();
            existingItem.Quantity.Should().Be(3); // 1 + 2
            _mockCartItemRepository.Verify(r => r.Update(It.IsAny<CartItem>()), Times.Once);
        }

        #endregion

        #region UpdateCartItemAsync Tests

        [Fact]
        public async Task UpdateCartItemAsync_WithValidProductIdAndQuantity_UpdatesItem()
        {
            // Arrange
            var userId = "user-1";
            var productId = 1;
            var updateDto = new UpdateCartItemDto { Quantity = 5 };

            var product = new Product
            {
                Id = productId,
                Price = 100,
                StockQuantity = 10
            };

            var cartItem = new CartItem
            {
                ProductId = productId,
                Quantity = 2,
                UnitPrice = 100
            };

            var cart = new Cart
            {
                Id = 1,
                UserId = userId,
                CartItems = new List<CartItem> { cartItem }
            };

            var expectedCartDto = new CartDto
            {
                Id = 1,
                TotalPrice = 500,
                TotalItems = 5,
                Items = new List<CartItemDto>()
            };

            _mockProductRepository
                .Setup(r => r.GetByIdAsync(productId))
                .ReturnsAsync(product);

            _mockCartRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<ISpecifications<Cart, int>>()))
                .ReturnsAsync(cart);

            _mockCartItemRepository
                .Setup(r => r.Update(It.IsAny<CartItem>()))
                .Callback<CartItem>(ci => { }); // No-op callback

            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            _mockMapper
                .Setup(m => m.Map<CartDto>(It.IsAny<Cart>()))
                .Returns(expectedCartDto);

            // Act
            var result = await _cartService.UpdateCartItemAsync(userId, productId, updateDto);

            // Assert
            result.Should().NotBeNull();
            cartItem.Quantity.Should().Be(5);
            _mockCartItemRepository.Verify(r => r.Update(It.IsAny<CartItem>()), Times.Once);
        }

        [Fact]
        public async Task UpdateCartItemAsync_WithInvalidQuantity_ThrowsException()
        {
            // Arrange
            var userId = "user-1";
            var productId = 1;
            var updateDto = new UpdateCartItemDto { Quantity = 0 };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _cartService.UpdateCartItemAsync(userId, productId, updateDto)
            );
        }

        [Fact]
        public async Task UpdateCartItemAsync_WithEmptyCart_ThrowsException()
        {
            // Arrange
            var userId = "user-1";
            var productId = 1;
            var updateDto = new UpdateCartItemDto { Quantity = 5 };

            var product = new Product { Id = productId, StockQuantity = 10 };

            _mockProductRepository
                .Setup(r => r.GetByIdAsync(productId))
                .ReturnsAsync(product);

            _mockCartRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<ISpecifications<Cart, int>>()))
                .ReturnsAsync((Cart?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _cartService.UpdateCartItemAsync(userId, productId, updateDto)
            );
        }

        [Fact]
        public async Task UpdateCartItemAsync_WithProductNotInCart_ThrowsException()
        {
            // Arrange
            var userId = "user-1";
            var productId = 1;
            var updateDto = new UpdateCartItemDto { Quantity = 5 };

            var product = new Product { Id = productId, StockQuantity = 10 };

            var cart = new Cart
            {
                Id = 1,
                UserId = userId,
                CartItems = new List<CartItem>() // Empty cart
            };

            _mockProductRepository
                .Setup(r => r.GetByIdAsync(productId))
                .ReturnsAsync(product);

            _mockCartRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<ISpecifications<Cart, int>>()))
                .ReturnsAsync(cart);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _cartService.UpdateCartItemAsync(userId, productId, updateDto)
            );
        }

        #endregion

        #region RemoveFromCartAsync Tests

        [Fact]
        public async Task RemoveFromCartAsync_WithValidProductId_RemovesItem()
        {
            // Arrange
            var userId = "user-1";
            var productId = 1;

            var cartItem = new CartItem { ProductId = productId, Quantity = 2 };

            var cart = new Cart
            {
                Id = 1,
                UserId = userId,
                CartItems = new List<CartItem> { cartItem }
            };

            var expectedCartDto = new CartDto
            {
                Id = 1,
                TotalPrice = 0,
                TotalItems = 0,
                Items = new List<CartItemDto>()
            };

            _mockCartRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<ISpecifications<Cart, int>>()))
                .ReturnsAsync(cart);

            _mockCartItemRepository
                .Setup(r => r.Remove(It.IsAny<CartItem>()))
                .Callback<CartItem>(ci => { }); // No-op callback

            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            _mockMapper
                .Setup(m => m.Map<CartDto>(It.IsAny<Cart>()))
                .Returns(expectedCartDto);

            // Act
            var result = await _cartService.RemoveFromCartAsync(userId, productId);

            // Assert
            result.Should().NotBeNull();
            _mockCartItemRepository.Verify(r => r.Remove(It.IsAny<CartItem>()), Times.Once);
        }

        [Fact]
        public async Task RemoveFromCartAsync_WithEmptyCart_ThrowsException()
        {
            // Arrange
            var userId = "user-1";
            var productId = 1;

            _mockCartRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<ISpecifications<Cart, int>>()))
                .ReturnsAsync((Cart?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _cartService.RemoveFromCartAsync(userId, productId)
            );
        }

        [Fact]
        public async Task RemoveFromCartAsync_WithProductNotInCart_ThrowsException()
        {
            // Arrange
            var userId = "user-1";
            var productId = 1;

            var cart = new Cart
            {
                Id = 1,
                UserId = userId,
                CartItems = new List<CartItem>() // Empty
            };

            _mockCartRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<ISpecifications<Cart, int>>()))
                .ReturnsAsync(cart);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _cartService.RemoveFromCartAsync(userId, productId)
            );
        }

        #endregion

        #region ClearCartAsync Tests

        [Fact]
        public async Task ClearCartAsync_WithItemsInCart_ClearsAllItems()
        {
            // Arrange
            var userId = "user-1";
            var cartItems = new List<CartItem>
            {
                new CartItem { ProductId = 1, Quantity = 2 },
                new CartItem { ProductId = 2, Quantity = 1 }
            };

            var cart = new Cart
            {
                Id = 1,
                UserId = userId,
                CartItems = cartItems
            };

            _mockCartRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<ISpecifications<Cart, int>>()))
                .ReturnsAsync(cart);

            _mockCartItemRepository
                .Setup(r => r.RemoveRange(It.IsAny<IEnumerable<CartItem>>()))
                .Callback<IEnumerable<CartItem>>(items => { }); // No-op callback

            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _cartService.ClearCartAsync(userId);

            // Assert
            _mockCartItemRepository.Verify(r => r.RemoveRange(It.IsAny<IEnumerable<CartItem>>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task ClearCartAsync_WithEmptyCart_DoesNothing()
        {
            // Arrange
            var userId = "user-1";

            _mockCartRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<ISpecifications<Cart, int>>()))
                .ReturnsAsync((Cart?)null);

            // Act
            await _cartService.ClearCartAsync(userId);

            // Assert
            _mockCartItemRepository.Verify(r => r.RemoveRange(It.IsAny<IEnumerable<CartItem>>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task ClearCartAsync_WithCartButNoItems_DoesNothing()
        {
            // Arrange
            var userId = "user-1";
            var cart = new Cart
            {
                Id = 1,
                UserId = userId,
                CartItems = new List<CartItem>() // Empty
            };

            _mockCartRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<ISpecifications<Cart, int>>()))
                .ReturnsAsync(cart);

            // Act
            await _cartService.ClearCartAsync(userId);

            // Assert
            _mockCartItemRepository.Verify(r => r.RemoveRange(It.IsAny<IEnumerable<CartItem>>()), Times.Never);
        }

        #endregion
    }
}

