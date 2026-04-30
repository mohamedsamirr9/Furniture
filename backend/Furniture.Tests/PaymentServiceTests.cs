using Furniture.Domain.InterfacesRepositories;
using Furniture.Domain.Models;
using Furniture.Domain.Models.Enum;
using Furniture.Services.Implementations;
using Furniture.Services.Specifications;
using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos.Payment;
using Microsoft.Extensions.Configuration;
using Moq;
using FluentAssertions;

namespace Furniture.Tests
{
    public class PaymentServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly Mock<ISellerPaymentService> _mockSellerPaymentService;
        private readonly Mock<IGenaricRepository<Order, int>> _mockOrderRepository;
        private readonly Mock<IGenaricRepository<Payment, int>> _mockPaymentRepository;
        private readonly Mock<IGenaricRepository<SellerPayout, int>> _mockPayoutRepository;
        private readonly Mock<IGenaricRepository<SellerProfile, int>> _mockSellerProfileRepository;
        private readonly PaymentService _paymentService;

        public PaymentServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockConfig = new Mock<IConfiguration>();
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockSellerPaymentService = new Mock<ISellerPaymentService>();
            _mockOrderRepository = new Mock<IGenaricRepository<Order, int>>();
            _mockPaymentRepository = new Mock<IGenaricRepository<Payment, int>>();
            _mockPayoutRepository = new Mock<IGenaricRepository<SellerPayout, int>>();
            _mockSellerProfileRepository = new Mock<IGenaricRepository<SellerProfile, int>>();

            _mockUnitOfWork
                .Setup(u => u.GetRepository<Order, int>())
                .Returns(_mockOrderRepository.Object);

            _mockUnitOfWork
                .Setup(u => u.GetRepository<Payment, int>())
                .Returns(_mockPaymentRepository.Object);

            _mockUnitOfWork
                .Setup(u => u.GetRepository<SellerPayout, int>())
                .Returns(_mockPayoutRepository.Object);

            _mockUnitOfWork
                .Setup(u => u.GetRepository<SellerProfile, int>())
                .Returns(_mockSellerProfileRepository.Object);

            var fakeHttpClient = CreateFakeHttpClient();
            _mockHttpClientFactory
                .Setup(f => f.CreateClient("Paymob"))
                .Returns(fakeHttpClient);

            _paymentService = new PaymentService(
                _mockUnitOfWork.Object,
                _mockConfig.Object,
                _mockHttpClientFactory.Object,
                _mockSellerPaymentService.Object
            );
        }

        // ============================================================
        // CreatePaymentAsync Tests
        // ============================================================

        [Fact]
        public async Task CreatePaymentAsync_WithOrderNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            _mockOrderRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<ISpecifications<Order, int>>()))
                .ReturnsAsync((Order?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _paymentService.CreatePaymentAsync(999, "user-1")
            );

            exception.Message.Should().Contain("Order not found");

            _mockOrderRepository.Verify(
                r => r.GetByIdAsync(It.IsAny<ISpecifications<Order, int>>()),
                Times.Once
            );
        }

        [Fact]
        public async Task CreatePaymentAsync_WithInvalidOrderStatus_ThrowsInvalidOperationException()
        {
            // Arrange
            var order = new Order
            {
                Id = 1,
                UserId = "user-1",
                Status = OrderStatus.Delivered,
                OrderItems = new List<OrderItem>()
            };

            _mockOrderRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<ISpecifications<Order, int>>()))
                .ReturnsAsync(order);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _paymentService.CreatePaymentAsync(1, "user-1")
            );

            exception.Message.Should().Contain("not ready for payment");
        }

        [Fact]
        public async Task CreatePaymentAsync_WithCompletedPaymentExists_ThrowsInvalidOperationException()
        {
            // Arrange
            var order = new Order
            {
                Id = 1,
                UserId = "user-1",
                Status = OrderStatus.Pending,
                OrderItems = new List<OrderItem>()
            };

            var existingPayment = new Payment
            {
                OrderId = 1,
                Status = PaymentStatus.Completed
            };

            _mockOrderRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<ISpecifications<Order, int>>()))
                .ReturnsAsync(order);

            _mockPaymentRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<ISpecifications<Payment, int>>()))
                .ReturnsAsync(existingPayment);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _paymentService.CreatePaymentAsync(1, "user-1")
            );

            exception.Message.Should().Contain("already paid");
        }

        // ============================================================
        // VerifyPaymentAsync Tests
        // ============================================================

        [Fact]
        public async Task VerifyPaymentAsync_WithCompletedPayment_ReturnsTrue()
        {
            // Arrange
            var payment = new Payment
            {
                OrderId = 1,
                Status = PaymentStatus.Completed,
                PaidAt = DateTime.UtcNow
            };

            _mockPaymentRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<ISpecifications<Payment, int>>()))
                .ReturnsAsync(payment);

            // Act
            var result = await _paymentService.VerifyPaymentAsync(1);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task VerifyPaymentAsync_WithPendingPayment_ReturnsFalse()
        {
            // Arrange
            var payment = new Payment
            {
                OrderId = 1,
                Status = PaymentStatus.Pending
            };

            _mockPaymentRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<ISpecifications<Payment, int>>()))
                .ReturnsAsync(payment);

            // Act
            var result = await _paymentService.VerifyPaymentAsync(1);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task VerifyPaymentAsync_WithNullPayment_ReturnsFalse()
        {
            // Arrange
            _mockPaymentRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<ISpecifications<Payment, int>>()))
                .ReturnsAsync((Payment?)null);

            // Act
            var result = await _paymentService.VerifyPaymentAsync(999);

            // Assert
            result.Should().BeFalse();
        }

        // ============================================================
        // HandlePaymentCallbackAsync Tests
        // ============================================================

        [Fact]
        public async Task HandlePaymentCallbackAsync_WithFailedCallback_ReturnsFalse()
        {
            // Arrange
            var callback = new PaymobCallbackDTO
            {
                Success = false,
                OrderId = 1
            };

            // Act
            var result = await _paymentService.HandlePaymentCallbackAsync(callback, "hmac");

            // Assert
            result.Should().BeFalse();

            _mockPaymentRepository.Verify(
                r => r.GetByIdAsync(It.IsAny<ISpecifications<Payment, int>>()),
                Times.Never
            );
        }

        [Fact]
        public async Task HandlePaymentCallbackAsync_WithPaymentNotFound_ReturnsFalse()
        {
            // Arrange
            var callback = new PaymobCallbackDTO
            {
                Success = true,
                OrderId = 999
            };

            _mockPaymentRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<ISpecifications<Payment, int>>()))
                .ReturnsAsync((Payment?)null);

            // Act
            var result = await _paymentService.HandlePaymentCallbackAsync(callback, "hmac");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task HandlePaymentCallbackAsync_WithAlreadyCompletedPayment_ReturnsTrue()
        {
            // Arrange
            var callback = new PaymobCallbackDTO
            {
                Success = true,
                OrderId = 1
            };

            var payment = new Payment
            {
                OrderId = 1,
                Status = PaymentStatus.Completed,
                PaidAt = DateTime.UtcNow
            };

            _mockPaymentRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<ISpecifications<Payment, int>>()))
                .ReturnsAsync(payment);

            // Act
            var result = await _paymentService.HandlePaymentCallbackAsync(callback, "hmac");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task HandlePaymentCallbackAsync_WithValidCallback_CallsProcessPayoutsForOrder()
        {
            // Arrange
            var orderId = 1;
            var callback = new PaymobCallbackDTO
            {
                Success = true,
                OrderId = orderId,
                TransactionId = "txn-123456"
            };

            var payment = new Payment
            {
                Id = 1,
                OrderId = orderId,
                Status = PaymentStatus.Pending
            };

            var order = new Order
            {
                Id = orderId,
                Status = OrderStatus.Pending,
                UserId = "user-1"
            };

            var payouts = new List<SellerPayout>();

            _mockPaymentRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<ISpecifications<Payment, int>>()))
                .ReturnsAsync(payment);

            _mockOrderRepository
                .Setup(r => r.GetByIdAsync(orderId))
                .ReturnsAsync(order);

            _mockPayoutRepository
                .Setup(r => r.GetAllAsync(It.IsAny<ISpecifications<SellerPayout, int>>()))
                .ReturnsAsync(payouts);

            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            _mockSellerPaymentService
                .Setup(s => s.ProcessPayoutsForOrderAsync(It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _paymentService.HandlePaymentCallbackAsync(callback, "hmac");

            // Assert
            result.Should().BeTrue();

            _mockSellerPaymentService.Verify(
                s => s.ProcessPayoutsForOrderAsync(orderId),
                Times.Once
            );
        }

        // ============================================================
        // Helper Methods
        // ============================================================

        private HttpClient CreateFakeHttpClient()
        {
            var handler = new FakePaymobHttpMessageHandler();
            return new HttpClient(handler)
            {
                BaseAddress = new Uri("https://api.paymob.com/"),
                Timeout = TimeSpan.FromSeconds(30)
            };
        }
    }

    public class FakePaymobHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var content = new StringContent(
                """{ "token": "fake-auth-token", "id": 123456 }""",
                System.Text.Encoding.UTF8,
                "application/json"
            );

            var response = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = content
            };

            return Task.FromResult(response);
        }
    }}