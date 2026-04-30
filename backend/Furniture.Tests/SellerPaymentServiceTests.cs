using Furniture.Domain.InterfacesRepositories;
using Furniture.Domain.Models;
using Furniture.Domain.Models.Enum;
using Furniture.Services.Implementations;
using Furniture.Services.Specifications;
using Furniture.Services.Specifications.Seller;
using Furniture.shared.Dtos.Payment;
using Furniture.shared.Dtos.Seller;
using Microsoft.Extensions.Configuration;
using Moq;
using FluentAssertions;

namespace Furniture.Tests
{
    public class SellerPaymentServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly Mock<IGenaricRepository<ApplicationUser, string>> _mockUserRepository;
        private readonly Mock<IGenaricRepository<SellerProfile, int>> _mockSellerProfileRepository;
        private readonly Mock<IGenaricRepository<SellerPayout, int>> _mockPayoutRepository;
        private readonly SellerPaymentService _sellerPaymentService;

        public SellerPaymentServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockConfig = new Mock<IConfiguration>();
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockUserRepository = new Mock<IGenaricRepository<ApplicationUser, string>>();
            _mockSellerProfileRepository = new Mock<IGenaricRepository<SellerProfile, int>>();
            _mockPayoutRepository = new Mock<IGenaricRepository<SellerPayout, int>>();

            _mockUnitOfWork
                .Setup(u => u.GetRepository<ApplicationUser, string>())
                .Returns(_mockUserRepository.Object);

            _mockUnitOfWork
                .Setup(u => u.GetRepository<SellerProfile, int>())
                .Returns(_mockSellerProfileRepository.Object);

            _mockUnitOfWork
                .Setup(u => u.GetRepository<SellerPayout, int>())
                .Returns(_mockPayoutRepository.Object);

            _sellerPaymentService = new SellerPaymentService(
                _mockUnitOfWork.Object,
                _mockConfig.Object,
                _mockHttpClientFactory.Object
            );
        }

        // ============================================================
        // CreateSellerProfileAsync Tests
        // ============================================================

        [Fact]
        public async Task CreateSellerProfileAsync_WithUserNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            var userId = "nonexistent-user";
            var dto = new CreateSellerProfileDTO
            {
                StoreName = "Test Store",
                BankName = "Test Bank",
                BankAccountNumber = "123456789",
                BankCode = "001",
                NationalId = "123456789"
            };

            _mockUserRepository
                .Setup(r => r.GetByIdAsync(userId))
                .ReturnsAsync((ApplicationUser?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sellerPaymentService.CreateSellerProfileAsync(userId, dto)
            );

            exception.Message.Should().Contain("User not found");
            _mockUserRepository.Verify(r => r.GetByIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task CreateSellerProfileAsync_WithNonSellerUser_ThrowsInvalidOperationException()
        {
            // Arrange
            var userId = "user-1";
            var dto = new CreateSellerProfileDTO { StoreName = "Test Store" };

            var user = new ApplicationUser
            {
                Id = userId,
                Role = Roles.buyer
            };

            _mockUserRepository
                .Setup(r => r.GetByIdAsync(userId))
                .ReturnsAsync(user);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sellerPaymentService.CreateSellerProfileAsync(userId, dto)
            );

            exception.Message.Should().Contain("not a Seller");
        }

        [Fact]
        public async Task CreateSellerProfileAsync_WithExistingProfile_ThrowsInvalidOperationException()
        {
            // Arrange
            var userId = "seller-1";
            var dto = new CreateSellerProfileDTO { StoreName = "Test Store" };

            var user = new ApplicationUser { Id = userId, Role = Roles.seller };
            var existingProfile = new SellerProfile { Id = 1, UserId = userId };

            _mockUserRepository
                .Setup(r => r.GetByIdAsync(userId))
                .ReturnsAsync(user);

            _mockSellerProfileRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<ISpecifications<SellerProfile, int>>()))
                .ReturnsAsync(existingProfile);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sellerPaymentService.CreateSellerProfileAsync(userId, dto)
            );

            exception.Message.Should().Contain("already exists");
        }

        [Fact]
        public async Task CreateSellerProfileAsync_WithValidData_ReturnsCreatedProfile()
        {
            // Arrange
            var userId = "seller-1";
            var dto = new CreateSellerProfileDTO
            {
                StoreName = "New Store",
                StoreDescription = "New Description",
                BankName = "Test Bank",
                BankAccountNumber = "123456789",
                BankCode = "001",
                NationalId = "123456789"
            };

            var user = new ApplicationUser { Id = userId, Role = Roles.seller };

            _mockUserRepository
                .Setup(r => r.GetByIdAsync(userId))
                .ReturnsAsync(user);

            _mockSellerProfileRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<ISpecifications<SellerProfile, int>>()))
                .ReturnsAsync((SellerProfile?)null);

            _mockSellerProfileRepository
                .Setup(r => r.AddAsync(It.IsAny<SellerProfile>()))
                .Returns(Task.CompletedTask);

            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _sellerPaymentService.CreateSellerProfileAsync(userId, dto);

            // Assert
            result.Should().NotBeNull();
            result.StoreName.Should().Be(dto.StoreName);
            result.IsVerified.Should().BeFalse();
            result.CommissionRate.Should().Be(10m);

            _mockSellerProfileRepository.Verify(
                r => r.AddAsync(It.Is<SellerProfile>(p => p.UserId == userId)),
                Times.Once
            );
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        // ============================================================
        // GetMyProfileAsync Tests
        // ============================================================

        [Fact]
        public async Task GetMyProfileAsync_WithNoProfile_ReturnsNull()
        {
            // Arrange
            _mockSellerProfileRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<ISpecifications<SellerProfile, int>>()))
                .ReturnsAsync((SellerProfile?)null);

            // Act
            var result = await _sellerPaymentService.GetMyProfileAsync("seller-1");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetMyProfileAsync_WithExistingProfile_ReturnsMappedProfile()
        {
            // Arrange
            var profile = new SellerProfile
            {
                Id = 1,
                UserId = "seller-1",
                StoreName = "My Store",
                StoreDescription = "My Description",
                IsVerified = true,
                CommissionRate = 10m
            };

            _mockSellerProfileRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<ISpecifications<SellerProfile, int>>()))
                .ReturnsAsync(profile);

            // Act
            var result = await _sellerPaymentService.GetMyProfileAsync("seller-1");

            // Assert
            result.Should().NotBeNull();
            result!.StoreName.Should().Be(profile.StoreName);
            result.IsVerified.Should().BeTrue();
            result.CommissionRate.Should().Be(10m);
        }

        // ============================================================
        // VerifySellerAsync Tests
        // ============================================================

        [Fact]
        public async Task VerifySellerAsync_WithSellerNotFound_ReturnsFalse()
        {
            // Arrange
            _mockSellerProfileRepository
                .Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((SellerProfile?)null);

            // Act
            var result = await _sellerPaymentService.VerifySellerAsync(999);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task VerifySellerAsync_WithAlreadyVerifiedSeller_ThrowsInvalidOperationException()
        {
            // Arrange
            var seller = new SellerProfile { Id = 1, IsVerified = true };

            _mockSellerProfileRepository
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(seller);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sellerPaymentService.VerifySellerAsync(1)
            );

            exception.Message.Should().Contain("already verified");
        }

        [Fact]
        public async Task VerifySellerAsync_WithUnverifiedSeller_ReturnsTrue()
        {
            // Arrange
            var seller = new SellerProfile
            {
                Id = 1,
                IsVerified = false,
                CommissionRate = 10m
            };

            _mockSellerProfileRepository
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(seller);

            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _sellerPaymentService.VerifySellerAsync(1);

            // Assert
            result.Should().BeTrue();
            seller.IsVerified.Should().BeTrue();

            _mockSellerProfileRepository.Verify(
                r => r.Update(It.IsAny<SellerProfile>()),
                Times.Once
            );
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        // ============================================================
        // GetEarningsAsync Tests
        // ============================================================

        [Fact]
        public async Task GetEarningsAsync_WithNoProfile_ThrowsInvalidOperationException()
        {
            // Arrange
            _mockSellerProfileRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<ISpecifications<SellerProfile, int>>()))
                .ReturnsAsync((SellerProfile?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sellerPaymentService.GetEarningsAsync("seller-1")
            );

            exception.Message.Should().Contain("not found");
        }

        [Fact]
        public async Task GetEarningsAsync_WithPayouts_ReturnsCorrectTotals()
        {
            // Arrange
            var sellerProfile = new SellerProfile { Id = 1, UserId = "seller-1" };

            var payouts = new List<SellerPayout>
            {
                new SellerPayout
                {
                    OrderItemsTotal = 1000m,
                    CommissionAmount = 100m,
                    NetAmount = 900m,
                    Status = PayoutStatus.Completed
                },
                new SellerPayout
                {
                    OrderItemsTotal = 2000m,
                    CommissionAmount = 200m,
                    NetAmount = 1800m,
                    Status = PayoutStatus.Pending
                }
            };

            _mockSellerProfileRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<ISpecifications<SellerProfile, int>>()))
                .ReturnsAsync(sellerProfile);

            _mockPayoutRepository
                .Setup(r => r.GetAllAsync(It.IsAny<ISpecifications<SellerPayout, int>>()))
                .ReturnsAsync(payouts);

            // Act
            var result = await _sellerPaymentService.GetEarningsAsync("seller-1");

            // Assert
            result.TotalSales.Should().Be(3000m);
            result.TotalCommission.Should().Be(300m);
            result.NetEarnings.Should().Be(2700m);
            result.PaidAmount.Should().Be(900m);
            result.PendingAmount.Should().Be(1800m);
        }

        [Fact]
        public async Task GetEarningsAsync_WithNoPayouts_ReturnsZeroTotals()
        {
            // Arrange
            var sellerProfile = new SellerProfile { Id = 1, UserId = "seller-1" };

            _mockSellerProfileRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<ISpecifications<SellerProfile, int>>()))
                .ReturnsAsync(sellerProfile);

            _mockPayoutRepository
                .Setup(r => r.GetAllAsync(It.IsAny<ISpecifications<SellerPayout, int>>()))
                .ReturnsAsync(new List<SellerPayout>());

            // Act
            var result = await _sellerPaymentService.GetEarningsAsync("seller-1");

            // Assert
            result.TotalSales.Should().Be(0m);
            result.TotalCommission.Should().Be(0m);
            result.NetEarnings.Should().Be(0m);
            result.PaidAmount.Should().Be(0m);
            result.PendingAmount.Should().Be(0m);
        }

        // ============================================================
        // RetryFailedPayoutAsync Tests
        // ============================================================

        [Fact]
        public async Task RetryFailedPayoutAsync_WithPayoutNotFound_ReturnsFalse()
        {
            // Arrange
            _mockPayoutRepository
                .Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((SellerPayout?)null);

            // Act
            var result = await _sellerPaymentService.RetryFailedPayoutAsync(999);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task RetryFailedPayoutAsync_WithNonFailedPayout_ReturnsFalse()
        {
            // Arrange
            var payout = new SellerPayout { Id = 1, Status = PayoutStatus.Pending };

            _mockPayoutRepository
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(payout);

            // Act
            var result = await _sellerPaymentService.RetryFailedPayoutAsync(1);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task RetryFailedPayoutAsync_WithFailedPayout_UpdatesPayoutStatus()
        {
            // Arrange
            var payout = new SellerPayout
            {
                Id = 1,
                Status = PayoutStatus.Failed,
                FailureReason = "Insufficient balance",
                SellerProfileId = 1
            };

            _mockPayoutRepository
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(payout);

            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            _mockSellerProfileRepository
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync((SellerProfile?)null);

            // Act
            await _sellerPaymentService.RetryFailedPayoutAsync(1);

            // Assert
            _mockPayoutRepository.Verify(
                r => r.Update(It.IsAny<SellerPayout>()),
                Times.AtLeastOnce
            );
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.AtLeastOnce);
        }
    }
}