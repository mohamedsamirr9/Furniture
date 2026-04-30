using Furniture.presentation.Controllers;
using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos.Payment;
using Microsoft.AspNetCore.Mvc;
using Moq;
using FluentAssertions;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Furniture.Tests
{
    public class PaymentsControllerTests
    {
        private readonly Mock<IPaymentService> _mockPaymentService;
        private readonly PaymentsController _controller;

        public PaymentsControllerTests()
        {
            _mockPaymentService = new Mock<IPaymentService>();
            _controller = new PaymentsController(_mockPaymentService.Object);
        }

        private void SetupControllerContext(string? userId = null)
        {
            var claims = new List<Claim>();
            
            if (!string.IsNullOrEmpty(userId))
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
            }

            var identity = new ClaimsIdentity(claims, "TestScheme");
            var principal = new ClaimsPrincipal(identity);

            var httpContext = new DefaultHttpContext { User = principal };
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        #region CreatePayment Tests

        [Fact]
        public async Task CreatePayment_WithoutUserIdClaim_ReturnsUnauthorized()
        {
            // Arrange
            SetupControllerContext(userId: null); // No user ID claim
            var dto = new CreatePaymentRequestDTO { OrderId = 1 };

            // Act
            var result = await _controller.CreatePayment(dto);

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task CreatePayment_WithValidData_ReturnsOk()
        {
            // Arrange
            SetupControllerContext(userId: "user-1");
            var dto = new CreatePaymentRequestDTO { OrderId = 1 };

            var paymentResponse = new PaymentResponseDTO
            {
                PaymentUrl = "https://checkout.paymob.com/fake-token",
                PaymentToken = "fake-token",
                OrderId = 1,
                Amount = 1000,
                Message = "Redirecting to payment gateway"
            };

            _mockPaymentService
                .Setup(s => s.CreatePaymentAsync(dto.OrderId, "user-1"))
                .ReturnsAsync(paymentResponse);

            // Act
            var result = await _controller.CreatePayment(dto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().Be(paymentResponse);
            _mockPaymentService.Verify(s => s.CreatePaymentAsync(dto.OrderId, "user-1"), Times.Once);
        }

        [Fact]
        public async Task CreatePayment_WithServiceThrowingInvalidOperationException_ReturnsBadRequest()
        {
            // Arrange
            SetupControllerContext(userId: "user-1");
            var dto = new CreatePaymentRequestDTO { OrderId = 999 };

            _mockPaymentService
                .Setup(s => s.CreatePaymentAsync(dto.OrderId, "user-1"))
                .ThrowsAsync(new InvalidOperationException("Order not found"));

            // Act
            var result = await _controller.CreatePayment(dto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().NotBeNull();
        }

        #endregion

        #region VerifyPayment Tests

        [Fact]
        public async Task VerifyPayment_WithValidOrderId_ReturnsOkWithIsPaidTrue()
        {
            // Arrange
            var orderId = 1;

            _mockPaymentService
                .Setup(s => s.VerifyPaymentAsync(orderId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.VerifyPayment(orderId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().NotBeNull();

            // Check if the response contains isPaid property
            var responseType = okResult.Value.GetType();
            var isPaidProperty = responseType.GetProperty("isPaid");
            isPaidProperty.Should().NotBeNull();
            isPaidProperty!.GetValue(okResult.Value).Should().Be(true);
        }

        [Fact]
        public async Task VerifyPayment_WithUnpaidOrder_ReturnsOkWithIsPaidFalse()
        {
            // Arrange
            var orderId = 1;

            _mockPaymentService
                .Setup(s => s.VerifyPaymentAsync(orderId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.VerifyPayment(orderId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().NotBeNull();

            // Check if the response contains isPaid property
            var responseType = okResult.Value.GetType();
            var isPaidProperty = responseType.GetProperty("isPaid");
            isPaidProperty.Should().NotBeNull();
            isPaidProperty!.GetValue(okResult.Value).Should().Be(false);
        }

        #endregion
    }
}

