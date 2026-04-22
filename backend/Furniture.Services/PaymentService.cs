using System.Net.Http.Json;
using System.Security.Cryptography; 
using System.Text;
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
        private readonly ISellerPaymentService _sellerPaymentService;

        public PaymentService(
            IUnitOfWork unitOfWork,
            IConfiguration config,
            IHttpClientFactory httpClientFactory,
            ISellerPaymentService sellerPaymentService)
        {
            _unitOfWork = unitOfWork;
            _config = config;
            _httpClient = httpClientFactory.CreateClient("Paymob");
            _sellerPaymentService = sellerPaymentService;
        }

        
        public async Task<PaymentResponseDTO> CreatePaymentAsync(int orderId, string userId)
        {
            var order = await GetOrderAsync(orderId, userId);

            ValidateOrderForPayment(order);

            var existingPayment = await GetExistingPaymentAsync(orderId);

            if (existingPayment?.Status == PaymentStatus.Completed)
                throw new InvalidOperationException("Order is already paid");

            var sellerPayouts = await BuildSellerPayoutsAsync(order, orderId);

            var (paymentToken, paymobOrderId) = await CreatePaymobPaymentAsync(order);

            await SavePaymentAsync(existingPayment, orderId, order, paymentToken, paymobOrderId);

            await SavePayoutsAsync(orderId, sellerPayouts, paymentToken);

            await _unitOfWork.SaveChangesAsync();

            return BuildPaymentResponse(orderId, order, paymentToken);
        }

        public async Task<bool> HandlePaymentCallbackAsync(PaymobCallbackDTO callback, string hmac)
        {
            if (!VerifyHmac(callback, hmac))
                return false;

            if (!callback.Success)
                return false;

            var payment = await GetPaymentByPaymobOrderIdAsync(callback.OrderId.ToString());

            if (payment == null)
                payment = await GetPaymentByMerchantOrderIdStoredAsync(callback.MerchantOrderId);

            if (payment == null)
                payment = await GetPaymentByMerchantOrderIdParsedAsync(callback.MerchantOrderId);

            if (payment == null)
                return false;

            if (payment.Status == PaymentStatus.Completed)
                return true;

            await CompletePaymentAsync(payment, callback);

            await _unitOfWork.SaveChangesAsync();

            await _sellerPaymentService.ProcessPayoutsForOrderAsync(payment.OrderId);

            return true;
        }

        public async Task<bool> VerifyPaymentAsync(int orderId)
        {
            var spec = new PaymentByOrderIdSpecification(orderId);
            var payment = await _unitOfWork.GetRepository<Payment, int>()
                .GetByIdAsync(spec);

            return payment?.Status == PaymentStatus.Completed;
        }

        // ============================================================
        // Private 
        // ============================================================

        private async Task<Order> GetOrderAsync(int orderId, string userId)
        {
            var spec = new OrderWithItemsSpecification(orderId, userId);
            var order = await _unitOfWork.GetRepository<Order, int>()
                .GetByIdAsync(spec);

            if (order == null)
                throw new InvalidOperationException("Order not found");

            return order;
        }

        private static void ValidateOrderForPayment(Order order)
        {
            if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Accepted)
                throw new InvalidOperationException("Order is not ready for payment");
        }

        private async Task<Payment?> GetExistingPaymentAsync(int orderId)
        {
            var spec = new PaymentByOrderIdSpecification(orderId);
            return await _unitOfWork.GetRepository<Payment, int>()
                .GetByIdAsync(spec);
        }

        private async Task<Payment?> GetPaymentByPaymobOrderIdAsync(string paymobOrderId)
        {
            var spec = new PaymentByPaymobOrderIdSpecification(paymobOrderId);
            return await _unitOfWork.GetRepository<Payment, int>()
                .GetByIdAsync(spec);
        }

        private async Task<Payment?> GetPaymentByMerchantOrderIdStoredAsync(string merchantOrderId)
        {
            if (string.IsNullOrWhiteSpace(merchantOrderId))
                return null;

            var spec = new PaymentByMerchantOrderIdSpecification(merchantOrderId);
            return await _unitOfWork.GetRepository<Payment, int>()
                .GetByIdAsync(spec);
        }

        private async Task<Payment?> GetPaymentByMerchantOrderIdParsedAsync(string merchantOrderId)
        {
            var orderId = TryExtractOrderIdFromMerchantOrderId(merchantOrderId);
            if (!orderId.HasValue)
                return null;

            var spec = new PaymentByOrderIdSpecification(orderId.Value);
            return await _unitOfWork.GetRepository<Payment, int>()
                .GetByIdAsync(spec);
        }

        private static int? TryExtractOrderIdFromMerchantOrderId(string merchantOrderId)
        {
            if (string.IsNullOrWhiteSpace(merchantOrderId))
            {
                return null;
            }

            var parts = merchantOrderId.Split('-', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3)
            {
                return null;
            }

            if (!parts[0].Equals("order", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (!int.TryParse(parts[1], out var orderId))
            {
                return null;
            }

            return orderId;
        }
 
        private async Task<List<SellerPayout>> BuildSellerPayoutsAsync(Order order, int orderId)
        {
            var payouts = new List<SellerPayout>();

            foreach (var group in order.OrderItems!.GroupBy(oi => oi.SellerId))
            {
                var sellerProfile = await GetSellerProfileAsync(group.Key);

                var itemsTotal = group.Sum(oi => oi.UnitPrice * oi.Quantity);
                var commission = itemsTotal * (sellerProfile.CommissionRate / 100m);

                payouts.Add(new SellerPayout
                {
                    SellerProfileId = sellerProfile.Id,
                    OrderId = orderId,
                    OrderItemsTotal = itemsTotal,
                    CommissionAmount = commission,
                    NetAmount = itemsTotal - commission,
                    Status = PayoutStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                });
            }

            return payouts;
        }

        private async Task<SellerProfile> GetSellerProfileAsync(string sellerId)
        {
            var spec = new SellerProfileByUserIdSpecification(sellerId);
            var profile = await _unitOfWork.GetRepository<SellerProfile, int>()
                .GetByIdAsync(spec);

            if (profile == null)
                throw new InvalidOperationException($"Seller {sellerId} profile not found");

            return profile;
        }
 

        private async Task SavePaymentAsync(
            Payment? existingPayment,
            int orderId,
            Order order,
            string paymentToken,
            string paymobOrderId)
        {
            var merchantOrderId = $"order-{order.Id}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";

            if (existingPayment != null)
            {
                existingPayment.PaymobTransactionId = paymentToken;
                existingPayment.PaymobOrderId = paymobOrderId;
                existingPayment.MerchantOrderId = merchantOrderId;
                existingPayment.CreatedAt = DateTime.UtcNow;
                _unitOfWork.GetRepository<Payment, int>().Update(existingPayment);
            }
            else
            {
                var payment = new Payment
                {
                    OrderId = orderId,
                    Amount = order.TotalPrice,
                    Currency = "EGP",
                    Method = PaymentMethod.Card,
                    Status = PaymentStatus.Pending,
                    PaymobTransactionId = paymentToken,
                    PaymobOrderId = paymobOrderId,
                    MerchantOrderId = merchantOrderId,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.GetRepository<Payment, int>().AddAsync(payment);
            }
        }

         private async Task SavePayoutsAsync(
            int orderId,
            List<SellerPayout> newPayouts,
            string paymentToken)
        {
            var existingPayoutsSpec = new SellerPayoutByOrderIdSpecification(orderId);
            var existingPayouts = (await _unitOfWork.GetRepository<SellerPayout, int>()
                .GetAllAsync(existingPayoutsSpec)).ToList();

            if (!existingPayouts.Any())
            {
                foreach (var payout in newPayouts)
                {
                    payout.PaymobTransactionId = paymentToken;
                    await _unitOfWork.GetRepository<SellerPayout, int>().AddAsync(payout);
                }
                return;
            }

            foreach (var payout in existingPayouts
                .Where(p => p.Status == PayoutStatus.Pending ||
                            p.Status == PayoutStatus.Failed))
            {
                payout.PaymobTransactionId = paymentToken;
                _unitOfWork.GetRepository<SellerPayout, int>().Update(payout);
            }
        }

        private PaymentResponseDTO BuildPaymentResponse(int orderId, Order order, string paymentToken)
        {
            var iframeId = _config["Paymob:IframeId"];
            var paymentUrl =
                $"https://accept.paymob.com/api/acceptance/iframes/{iframeId}?payment_token={paymentToken}";

            return new PaymentResponseDTO
            {
                PaymentUrl = paymentUrl,
                PaymentToken = paymentToken,
                OrderId = orderId,
                Amount = order.TotalPrice
            };
        }
 

        private async Task CompletePaymentAsync(Payment payment, PaymobCallbackDTO callback)
        {
            payment.Status = PaymentStatus.Completed;
            payment.PaidAt = DateTime.UtcNow;
            payment.PaymobTransactionId = callback.TransactionId;
            _unitOfWork.GetRepository<Payment, int>().Update(payment);

            var order = await _unitOfWork.GetRepository<Order, int>()
                .GetByIdAsync(payment.OrderId);

            if (order != null)
            {
                order.Status = OrderStatus.Paid;
                _unitOfWork.GetRepository<Order, int>().Update(order);
            }

            var payoutSpec = new SellerPayoutByOrderIdSpecification(payment.OrderId);
            var payouts = await _unitOfWork.GetRepository<SellerPayout, int>()
                .GetAllAsync(payoutSpec);

            foreach (var payout in payouts.Where(p => p.Status == PayoutStatus.Pending))
            {
                payout.Status = PayoutStatus.Processing;
                payout.PaymobTransactionId = callback.TransactionId;
                _unitOfWork.GetRepository<SellerPayout, int>().Update(payout);
            }
        }
 

        private async Task<(string PaymentKey, string PaymobOrderId)> CreatePaymobPaymentAsync(Order order)
        {
            var authToken = await GetAuthTokenAsync();
            var paymobOrderId = await CreatePaymobOrderAsync(authToken, order);
            var paymentKey = await GetPaymentKeyAsync(authToken, paymobOrderId, order);
            return (paymentKey, paymobOrderId.ToString());
        }

        private async Task<string> GetAuthTokenAsync()
        {
            var response = await _httpClient.PostAsJsonAsync(
                "https://accept.paymob.com/api/auth/tokens",
                new { api_key = _config["Paymob:ApiKey"] });

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<PaymobAuthResponse>();
            return result!.Token;
        }

        private async Task<int> CreatePaymobOrderAsync(string authToken, Order order)
        {
            var request = new
            {
                auth_token = authToken,
                delivery_needed = false,
                amount_cents = (int)Math.Round(order.TotalPrice * 100m),
                currency = "EGP",
                merchant_order_id = $"order-{order.Id}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                items = order.OrderItems!.Select(oi => new
                {
                    name = oi.Product?.NameEn ?? "Product",
                    amount_cents = (int)Math.Round(oi.UnitPrice * 100m),
                    description = oi.Product?.NameEn ?? "Furniture item",
                    quantity = oi.Quantity
                }).ToList()
            };

            var response = await _httpClient.PostAsJsonAsync(
                "https://accept.paymob.com/api/ecommerce/orders", request);

            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException(
                    $"Paymob Create Order failed ({(int)response.StatusCode}): {content}");

            var result = System.Text.Json.JsonSerializer.Deserialize<PaymobOrderResponse>(
                content,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return result!.Id;
        }

        private async Task<string> GetPaymentKeyAsync(string authToken, int paymobOrderId, Order order)
        {
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
                integration_id = int.Parse(_config["Paymob:IntegrationId"]!)
            };

            var response = await _httpClient.PostAsJsonAsync(
                "https://accept.paymob.com/api/acceptance/payment_keys", request);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<PaymobPaymentKeyResponse>();
            return result!.Token;
        }
 

#if DEBUG
        private bool VerifyHmac(PaymobCallbackDTO callback, string receivedHmac)
            => true;
#else
        private bool VerifyHmac(PaymobCallbackDTO callback, string receivedHmac)
        {
            var hmacSecret = _config["Paymob:HmacSecret"];
            if (string.IsNullOrEmpty(hmacSecret))
                return false;

            var dataString = string.Concat(
                callback.AmountCents,
                callback.CreatedAt,
                callback.Currency,
                callback.ErrorOccured,
                callback.HasParentTransaction,
                callback.Id,
                callback.IntegrationId,
                callback.IsCaptured,
                callback.IsRefundedTransaction,
                callback.IsStandalonePayment,
                callback.IsVoided,
                callback.OrderId,
                callback.OwnerUsername,
                callback.PendingStatus,
                callback.SourceDataPan,
                callback.SourceDataSubType,
                callback.SourceDataType,
                callback.Success);

            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(hmacSecret));
            var computedHmac = BitConverter.ToString(
                hmac.ComputeHash(Encoding.UTF8.GetBytes(dataString)))
                .Replace("-", "").ToLower();

            return computedHmac == receivedHmac.ToLower();
        }
#endif
    }
}
