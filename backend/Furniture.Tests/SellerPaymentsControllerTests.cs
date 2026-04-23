using Furniture.presentation.Controllers;
using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos.Payment;
using Furniture.shared.Dtos.Seller;
using Microsoft.AspNetCore.Mvc;
using Moq;
using FluentAssertions;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Furniture.Tests
{
    public class SellerPaymentsControllerTests
    {
        private readonly Mock<ISellerPaymentService> _mockSellerPaymentService;
        private readonly SellerPaymentsController _controller;

        public SellerPaymentsControllerTests()
        {
            _mockSellerPaymentService = new Mock<ISellerPaymentService>();
            _controller = new SellerPaymentsController(_mockSellerPaymentService.Object);
        }

        private void SetupControllerContext(string? userId = null, string role = "seller")
        {
            var claims = new List<Claim>();

            if (!string.IsNullOrEmpty(userId))
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
            }

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

        #region CreateProfile Tests

        [Fact]
        public async Task CreateProfile_WithoutUserIdClaim_ReturnsUnauthorized()
        {
            // Arrange
            SetupControllerContext(userId: null);
            var dto = new CreateSellerProfileDTO
            {
                StoreName = "Test Store",
                StoreDescription = "Test Description",
                BankName = "Test Bank",
                BankAccountNumber = "123456789",
                BankCode = "001",
                NationalId = "123456789"
            };

            // Act
            var result = await _controller.CreateProfile(dto);

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task CreateProfile_WithValidData_ReturnsOk()
        {
            // Arrange
            SetupControllerContext(userId: "seller-1");
            var dto = new CreateSellerProfileDTO
            {
                StoreName = "My Store",
                StoreDescription = "My Store Description",
                BankName = "Test Bank",
                BankAccountNumber = "123456789",
                BankCode = "001",
                NationalId = "123456789"
            };

            var createdProfile = new SellerProfileDTO
            {
                Id = 1,
                StoreName = dto.StoreName,
                StoreDescription = dto.StoreDescription,
                IsVerified = false,
                CommissionRate = 10m
            };

            _mockSellerPaymentService
                .Setup(s => s.CreateSellerProfileAsync("seller-1", dto))
                .ReturnsAsync(createdProfile);

            // Act
            var result = await _controller.CreateProfile(dto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().Be(createdProfile);
        }

        [Fact]
        public async Task CreateProfile_WithServiceThrowingInvalidOperationException_ReturnsBadRequest()
        {
            // Arrange
            SetupControllerContext(userId: "seller-1");
            var dto = new CreateSellerProfileDTO
            {
                StoreName = "Store",
                StoreDescription = "Description",
                BankName = "Bank",
                BankAccountNumber = "123",
                BankCode = "001",
                NationalId = "456"
            };

            _mockSellerPaymentService
                .Setup(s => s.CreateSellerProfileAsync("seller-1", dto))
                .ThrowsAsync(new InvalidOperationException("Seller profile already exists"));

            // Act
            var result = await _controller.CreateProfile(dto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        #endregion

        #region GetMyProfile Tests

        [Fact]
        public async Task GetMyProfile_WithoutUserIdClaim_ReturnsUnauthorized()
        {
            // Arrange
            SetupControllerContext(userId: null);

            // Act
            var result = await _controller.GetMyProfile();

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task GetMyProfile_WithNoProfile_ReturnsNotFound()
        {
            // Arrange
            SetupControllerContext(userId: "seller-1");

            _mockSellerPaymentService
                .Setup(s => s.GetMyProfileAsync("seller-1"))
                .ReturnsAsync((SellerProfileDTO?)null);

            // Act
            var result = await _controller.GetMyProfile();

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetMyProfile_WithExistingProfile_ReturnsOk()
        {
            // Arrange
            SetupControllerContext(userId: "seller-1");
            var profile = new SellerProfileDTO
            {
                Id = 1,
                StoreName = "My Store",
                StoreDescription = "My Description",
                IsVerified = true,
                CommissionRate = 10m
            };

            _mockSellerPaymentService
                .Setup(s => s.GetMyProfileAsync("seller-1"))
                .ReturnsAsync(profile);

            // Act
            var result = await _controller.GetMyProfile();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().Be(profile);
        }

        #endregion

        #region GetEarnings Tests

        [Fact]
        public async Task GetEarnings_WithoutUserIdClaim_ReturnsUnauthorized()
        {
            // Arrange
            SetupControllerContext(userId: null);

            // Act
            var result = await _controller.GetEarnings();

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task GetEarnings_WithValidData_ReturnsOk()
        {
            // Arrange
            SetupControllerContext(userId: "seller-1");
            var earnings = new SellerEarningsDTO
            {
                TotalSales = 5000m,
                TotalCommission = 500m,
                NetEarnings = 4500m,
                PendingAmount = 2000m,
                PaidAmount = 2500m
            };

            _mockSellerPaymentService
                .Setup(s => s.GetEarningsAsync("seller-1"))
                .ReturnsAsync(earnings);

            // Act
            var result = await _controller.GetEarnings();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().Be(earnings);
        }

        [Fact]
        public async Task GetEarnings_WithServiceThrowingInvalidOperationException_ReturnsBadRequest()
        {
            // Arrange
            SetupControllerContext(userId: "seller-1");

            _mockSellerPaymentService
                .Setup(s => s.GetEarningsAsync("seller-1"))
                .ThrowsAsync(new InvalidOperationException("Seller profile not found"));

            // Act
            var result = await _controller.GetEarnings();

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        #endregion

        #region GetAllSellers Tests

        [Fact]
        public async Task GetAllSellers_WithValidAdminRole_ReturnsOk()
        {
            // Arrange
            SetupControllerContext(userId: "admin-1", role: "admin");
            var sellers = new List<SellerProfileDTO>
            {
                new SellerProfileDTO { Id = 1, StoreName = "Store 1", IsVerified = true },
                new SellerProfileDTO { Id = 2, StoreName = "Store 2", IsVerified = false }
            };

            _mockSellerPaymentService
                .Setup(s => s.GetAllSellersAsync())
                .ReturnsAsync(sellers);

            // Act
            var result = await _controller.GetAllSellers();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().Be(sellers);
        }

        #endregion

        #region GetPendingSellers Tests

        [Fact]
        public async Task GetPendingSellers_WithValidAdminRole_ReturnsOk()
        {
            // Arrange
            SetupControllerContext(userId: "admin-1", role: "admin");
            var pendingSellers = new List<SellerProfileDTO>
            {
                new SellerProfileDTO { Id = 1, StoreName = "Store 1", IsVerified = false }
            };

            _mockSellerPaymentService
                .Setup(s => s.GetPendingSellersAsync())
                .ReturnsAsync(pendingSellers);

            // Act
            var result = await _controller.GetPendingSellers();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().Be(pendingSellers);
        }

        #endregion

        #region VerifySeller Tests

        [Fact]
        public async Task VerifySeller_WithNonExistentSeller_ReturnsNotFound()
        {
            // Arrange
            SetupControllerContext(userId: "admin-1", role: "admin");
            var sellerId = 999;

            _mockSellerPaymentService
                .Setup(s => s.VerifySellerAsync(sellerId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.VerifySeller(sellerId);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task VerifySeller_WithValidSeller_ReturnsOk()
        {
            // Arrange
            SetupControllerContext(userId: "admin-1", role: "admin");
            var sellerId = 1;

            _mockSellerPaymentService
                .Setup(s => s.VerifySellerAsync(sellerId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.VerifySeller(sellerId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task VerifySeller_WithServiceThrowingInvalidOperationException_ReturnsBadRequest()
        {
            // Arrange
            SetupControllerContext(userId: "admin-1", role: "admin");
            var sellerId = 1;

            _mockSellerPaymentService
                .Setup(s => s.VerifySellerAsync(sellerId))
                .ThrowsAsync(new InvalidOperationException("Seller is already verified"));

            // Act
            var result = await _controller.VerifySeller(sellerId);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        #endregion

        #region RetryPayout Tests

        [Fact]
        public async Task RetryPayout_WithFailedPayout_ReturnsOk()
        {
            // Arrange
            SetupControllerContext(userId: "admin-1", role: "admin");
            var payoutId = 1;

            _mockSellerPaymentService
                .Setup(s => s.RetryFailedPayoutAsync(payoutId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.RetryPayout(payoutId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task RetryPayout_WithNonFailedPayout_ReturnsBadRequest()
        {
            // Arrange
            SetupControllerContext(userId: "admin-1", role: "admin");
            var payoutId = 999;

            _mockSellerPaymentService
                .Setup(s => s.RetryFailedPayoutAsync(payoutId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.RetryPayout(payoutId);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        #endregion
    }
}

