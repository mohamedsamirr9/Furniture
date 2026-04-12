// PaymentService.cs

using System.Net.Http.Json;
using Furniture.Domain.InterfacesRepositories;
using Furniture.Domain.Models;
using Furniture.Domain.Models.Enum;
using Furniture.Services.Specifications;
using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos.Payment;
using Microsoft.Extensions.Configuration;

namespace Furniture.Services.Implementations
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;

        public PaymentService(
            IUnitOfWork unitOfWork,
            IConfiguration config,
            IHttpClientFactory httpClientFactory)
        {
            _unitOfWork = unitOfWork;
            _config = config;
            _httpClient = httpClientFactory.CreateClient("Paymob");
        }

        public async Task<PaymentResponseDTO> CreatePaymentAsync(int orderId, string userId)
        {
            var orderSpec = new OrderWithItemsSpecification(orderId, userId);
            var order = await _unitOfWork.GetRepository<Order, int>()
                .GetByIdAsync(orderSpec);

            if (order == null)
                throw new InvalidOperationException("Order not found");

            if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Accepted)
                throw new InvalidOperationException("Order is not ready for payment");

            var splits = new List<PaymobSplit>();
            var sellerPayouts = new List<SellerPayout>();

            var itemsBySeller = order.OrderItems!.GroupBy(oi => oi.SellerId);
            decimal platformCommissionTotal = 0;

            foreach (var group in itemsBySeller)
            {
                var sellerId = group.Key;
                var sellerProfileSpec = new SellerProfileByUserIdSpecification(sellerId);
                var sellerProfile = await _unitOfWork.GetRepository<SellerProfile, int>()
                    .GetByIdAsync(sellerProfileSpec);

                if (sellerProfile == null || string.IsNullOrEmpty(sellerProfile.PaymobMerchantId))
                    throw new InvalidOperationException($"Seller {sellerId} is not configured for payments");

                var itemsTotal = group.Sum(oi => oi.UnitPrice * oi.Quantity);
                var commission = itemsTotal * (sellerProfile.CommissionRate / 100m);
                var netAmount = itemsTotal - commission;
                platformCommissionTotal += commission;

                splits.Add(new PaymobSplit
                {
                    SubMerchantId = sellerProfile.PaymobMerchantId,
                    AmountCents = (int)(netAmount * 100)
                });

                sellerPayouts.Add(new SellerPayout
                {
                    SellerProfileId = sellerProfile.Id,
                    OrderId = orderId,
                    OrderItemsTotal = itemsTotal,
                    CommissionAmount = commission,
                    NetAmount = netAmount,
                    Status = PayoutStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                });
            }

            var platformMerchantId = _config["Paymob:PlatformMerchantId"];
            splits.Add(new PaymobSplit
            {
                SubMerchantId = platformMerchantId!,
                AmountCents = (int)(platformCommissionTotal * 100)
            });

            var paymentToken = await CreatePaymobPaymentAsync(order, splits);

            var payment = new Payment
            {
                OrderId = orderId,
                Amount = order.TotalPrice,
                Currency = "EGP",
                Method = PaymentMethod.Card,
                Status = PaymentStatus.Pending,
                PaymobTransactionId = paymentToken,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.GetRepository<Payment, int>().AddAsync(payment);

            foreach (var payout in sellerPayouts)
            {
                payout.PaymobTransactionId = paymentToken;
                await _unitOfWork.GetRepository<SellerPayout, int>().AddAsync(payout);
            }

            await _unitOfWork.SaveChangesAsync();

            var iframeId = _config["Paymob:IframeId"];
            var paymentUrl = $"https://accept.paymob.com/api/acceptance/iframes/{iframeId}?payment_token={paymentToken}";

            return new PaymentResponseDTO
            {
                PaymentUrl = paymentUrl,
                PaymentToken = paymentToken,
                OrderId = orderId,
                Amount = order.TotalPrice
            };
        }

        private async Task<string> CreatePaymobPaymentAsync(Order order, List<PaymobSplit> splits)
        {
            var authToken = await GetAuthTokenAsync();

            var paymobOrderId = await CreatePaymobOrderAsync(authToken, order, splits);

            var paymentKey = await GetPaymentKeyAsync(authToken, paymobOrderId, order);

            return paymentKey;
        }

        private async Task<string> GetAuthTokenAsync()
        {
            var apiKey = _config["Paymob:ApiKey"];
            var request = new { api_key = apiKey };

            var response = await _httpClient.PostAsJsonAsync(
                "https://accept.paymob.com/api/auth/tokens",
                request);

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<PaymobAuthResponse>();
            return result!.Token;
        }

        private async Task<int> CreatePaymobOrderAsync(string authToken, Order order, List<PaymobSplit> splits)
        {
            var request = new
            {
                auth_token = authToken,
                delivery_needed = "false",
                amount_cents = (int)(order.TotalPrice * 100),
                currency = "EGP",
                merchant_order_id = order.Id.ToString(),
                items = order.OrderItems!.Select(oi => new
                {
                    name = oi.Product?.Name ?? "Product",
                    amount_cents = (int)(oi.UnitPrice * 100),
                    quantity = oi.Quantity
                }).ToList(),
                sub_merchants = splits.Select(s => new
                {
                    sub_merchant_id = s.SubMerchantId,
                    amount_cents = s.AmountCents
                }).ToList()
            };

            var response = await _httpClient.PostAsJsonAsync(
                "https://accept.paymob.com/api/ecommerce/orders",
                request);

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<PaymobOrderResponse>();
            return result!.Id;
        }

        private async Task<string> GetPaymentKeyAsync(string authToken, int paymobOrderId, Order order)
        {
            var integrationId = _config["Paymob:IntegrationId"];

            var request = new
            {
                auth_token = authToken,
                amount_cents = (int)(order.TotalPrice * 100),
                expiration = 3600,
                order_id = paymobOrderId.ToString(),
                billing_data = new
                {
                    apartment = "NA",
                    email = order.User?.Email ?? "user@example.com",
                    floor = "NA",
                    first_name = order.User?.Name ?? "User",
                    street = order.ShippingAddress ?? "NA",
                    building = "NA",
                    phone_number = order.User?.PhoneNumber ?? "01000000000",
                    shipping_method = "NA",
                    postal_code = "NA",
                    city = "Cairo",
                    country = "EG",
                    last_name = "User",
                    state = "NA"
                },
                currency = "EGP",
                integration_id = int.Parse(integrationId!)
            };

            var response = await _httpClient.PostAsJsonAsync(
                "https://accept.paymob.com/api/acceptance/payment_keys",
                request);

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<PaymobPaymentKeyResponse>();
            return result!.Token;
        }

        public async Task<bool> HandlePaymentCallbackAsync(PaymobCallbackDTO callback)
        {
            if (!callback.Success)
                return false;

            var payments = await _unitOfWork.GetRepository<Payment, int>().GetAllAsync();
            var payment = payments.FirstOrDefault(p =>
                p.OrderId == callback.OrderId &&
                p.PaymobTransactionId == callback.TransactionId);

            if (payment == null)
                return false;

            payment.Status = PaymentStatus.Completed;
            payment.PaidAt = DateTime.UtcNow;
            _unitOfWork.GetRepository<Payment, int>().Update(payment);

            var order = await _unitOfWork.GetRepository<Order, int>()
                .GetByIdAsync(payment.OrderId);

            if (order != null)
            {
                order.Status = OrderStatus.Paid;
                _unitOfWork.GetRepository<Order, int>().Update(order);
            }

            var payouts = await _unitOfWork.GetRepository<SellerPayout, int>().GetAllAsync();
            var orderPayouts = payouts.Where(p => p.OrderId == payment.OrderId);

            foreach (var payout in orderPayouts)
            {
                payout.Status = PayoutStatus.Processing;
                _unitOfWork.GetRepository<SellerPayout, int>().Update(payout);
            }

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> VerifyPaymentAsync(int orderId)
        {
            var order = await _unitOfWork.GetRepository<Order, int>()
                .GetByIdAsync(orderId);

            return order?.Payment?.Status == PaymentStatus.Completed;
        }
    }

      
    internal class PaymobSplit
    {
        public string SubMerchantId { get; set; } = null!;
        public int AmountCents { get; set; }
    }

    internal class PaymobAuthResponse
    {
        public string Token { get; set; } = null!;
    }

    internal class PaymobOrderResponse
    {
        public int Id { get; set; }
    }

    internal class PaymobPaymentKeyResponse
    {
        public string Token { get; set; } = null!;
    }
}