using System.Net.Http.Json;
using System.Security.Cryptography; 
using System.Text;
using Furniture.Domain.InterfacesRepositories;
using Furniture.Domain.Models;
using Furniture.Domain.Models.Enum;
using Furniture.Services.Specifications.Order;
using Furniture.Services.Specifications;
using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos.Payment;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Furniture.Services.Implementations
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;
        private readonly ISellerPaymentService _sellerPaymentService;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            IUnitOfWork unitOfWork,
            IConfiguration config,
            IHttpClientFactory httpClientFactory,
            ISellerPaymentService sellerPaymentService,
            ILogger<PaymentService> logger)
        {
            _unitOfWork = unitOfWork;
            _config = config;
            _httpClient = httpClientFactory.CreateClient("Paymob");
            _sellerPaymentService = sellerPaymentService;
            _logger = logger;
        }

        
        public async Task<PaymentResponseDTO> CreatePaymentAsync(int orderId, string userId, string paymentMethod = "card")
        {
            var order = await GetOrderAsync(orderId, userId);
            var relatedOrders = await GetPaymentOrdersAsync(order, userId);
            ValidateOrdersForPayment(relatedOrders);

            var requestedMethod = paymentMethod.Equals("cash", StringComparison.OrdinalIgnoreCase)
                ? PaymentMethod.Cash
                : PaymentMethod.Card;

            var existingPayment = await GetExistingPaymentAsync(orderId);
            if (existingPayment == null && order.PaymentId.HasValue)
            {
                existingPayment = await _unitOfWork.GetRepository<Payment, int>()
                    .GetByIdAsync(order.PaymentId.Value);
            }

            if (existingPayment?.Status == PaymentStatus.Completed)
                throw new InvalidOperationException("Order is already paid");

            if (existingPayment != null && existingPayment.Status == PaymentStatus.Pending)
            {
                if (existingPayment.Method == PaymentMethod.Cash && requestedMethod == PaymentMethod.Cash)
                {
                    var hasMissingPayouts = await HasMissingSellerPayoutsAsync(relatedOrders);
                    if (hasMissingPayouts)
                    {
                        _logger.LogInformation(
                            "Cash payment exists but payouts are missing for one or more orders. Initializing payouts. PaymentId={PaymentId}, OrderId={OrderId}",
                            existingPayment.Id, orderId);
                        return await HandleCashPaymentAsync(relatedOrders, orderId, existingPayment);
                    }

                    return new PaymentResponseDTO
                    {
                        PaymentUrl = null,
                        PaymentToken = null,
                        OrderId = orderId,
                        Amount = relatedOrders.Sum(o => o.TotalPrice),
                        Message = "Cash payment already recorded for this order."
                    };
                }
            }

            await ValidateSellerNotBlockedAsync(relatedOrders);

            if (requestedMethod == PaymentMethod.Cash)
            {
                await ValidateCashExposureAsync(relatedOrders);
                return await HandleCashPaymentAsync(relatedOrders, orderId, existingPayment);
            }

            var merchantOrderId = $"order-{order.Id}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
            var totalAmount = relatedOrders.Sum(o => o.TotalPrice);
            var allOrderItems = relatedOrders.SelectMany(o => o.OrderItems ?? new List<OrderItem>()).ToList();
            var sellerPayouts = await BuildSellerPayoutsAsync(relatedOrders);

            var (paymentToken, paymobOrderId) =
                await CreatePaymobPaymentAsync(totalAmount, allOrderItems, order, merchantOrderId);

            await SavePaymentAsync(
                existingPayment,
                relatedOrders,
                totalAmount,
                paymentToken,
                paymobOrderId,
                merchantOrderId);

            await SavePayoutsAsync(relatedOrders.Select(o => o.Id).ToList(), sellerPayouts, paymentToken);

            await _unitOfWork.SaveChangesAsync();

            return BuildPaymentResponse(orderId, totalAmount, paymentToken);
        }
       public async Task<bool> HandlePaymentCallbackAsync(PaymobCallbackDTO callback, string hmac)
        {
            _logger.LogInformation(
                "Paymob callback received: OrderId={OrderId}, Success={Success}, TransactionId={TransactionId}",
                callback.order, callback.success, callback.id);

            if (!VerifyHmac(callback, hmac))
            {
                _logger.LogWarning("HMAC verification failed for OrderId={OrderId}", callback.order);
                return false;
            }

            if (!callback.success)
            {
                _logger.LogInformation("Payment not successful for OrderId={OrderId}", callback.order);
                await MarkPaymentAsFailedAsync(callback);
                return false;
            }

            var payment = await GetPaymentByPaymobOrderIdAsync(callback.order.ToString());

            if (payment == null && !string.IsNullOrWhiteSpace(callback.merchant_order_id))
            {
                payment = await GetPaymentByMerchantOrderIdStoredAsync(callback.merchant_order_id);
            }

            if (payment == null)
            {
                _logger.LogWarning(
                    "Payment not found for OrderId={PaymobOrderId}, MerchantOrderId={MerchantOrderId}",
                    callback.order, callback.merchant_order_id);
                return false;
            }

            _logger.LogInformation(
                "Found payment {PaymentId} for OrderId={InternalOrderId}, Status={Status}",
                payment.Id, payment.OrderId, payment.Status);

            if (payment.Status == PaymentStatus.Completed)
            {
                _logger.LogInformation(
                    "Payment already completed for OrderId={InternalOrderId}, acknowledging duplicate",
                    payment.OrderId);
                return true;
            }

            if (payment.Status == PaymentStatus.Cancelled)
            {
                _logger.LogInformation(
                    "Ignoring callback for cancelled payment on OrderId={InternalOrderId}",
                    payment.OrderId);
                return true;
            }

            var paymentOrders = await GetPaymentOrdersByPaymentIdAsync(payment.Id);
            if (paymentOrders.Any(o => o.Status == OrderStatus.Cancelled || o.Status == OrderStatus.Declined))
            {
                _logger.LogInformation("Ignoring callback because one or more linked orders are cancelled/declined. PaymentId={PaymentId}", payment.Id);
                return true;
            }

            await CompletePaymentAsync(payment, callback);
            await _unitOfWork.SaveChangesAsync();
            foreach (var oid in await GetPaymentOrderIdsAsync(payment))
            {
                await _sellerPaymentService.ProcessPayoutsForOrderAsync(oid);
            }

            _logger.LogInformation("Payment completed successfully for OrderId={InternalOrderId}", payment.OrderId);

            return true;
        }

        public async Task<bool> VerifyPaymentAsync(int orderId)
        {
            var spec = new PaymentByOrderIdSpecification(orderId);
            var payment = await _unitOfWork.GetRepository<Payment, int>()
                .GetByIdAsync(spec);

            return payment?.Status == PaymentStatus.Completed;
        }
        
        public async Task ConfirmCashPaymentAfterDeliveryAsync(int orderId)
        {
            _logger.LogInformation("ConfirmCashPaymentAfterDeliveryAsync started for OrderId={OrderId}", orderId);
            var payment = await GetExistingPaymentAsync(orderId);

            if (payment == null)
            {
                _logger.LogError("Cannot confirm cash payment for Order {OrderId}: Payment record missing.", orderId);
                return;
            }

            if (payment.Method != PaymentMethod.Cash)
            {
                _logger.LogInformation(
                    "Skipping cash commission for OrderId={OrderId}: Payment method is {Method}, not Cash.",
                    orderId, payment.Method);
                return;
            }

            if (payment.Status == PaymentStatus.Cancelled)
            {
                _logger.LogInformation("Skipping cash commission for OrderId={OrderId}: Payment is cancelled.", orderId);
                return;
            }

            var order = await _unitOfWork.GetRepository<Order, int>()
                .GetByIdAsync(orderId);

            if (order == null || order.Status != OrderStatus.Delivered)
            {
                _logger.LogInformation(
                    "Skipping cash commission for OrderId={OrderId}: Order missing or status is {Status}.",
                    orderId, order?.Status);
                return;
            }

            // For split cash orders sharing one payment, one delivered order may complete
            // the master payment before other sibling orders are delivered.
            // Keep processing per-order payouts even when payment is already completed.
            if (payment.Status != PaymentStatus.Completed)
            {
                payment.Status = PaymentStatus.Completed;
                payment.PaidAt = DateTime.UtcNow;
                _unitOfWork.GetRepository<Payment, int>().Update(payment);
            }

            var payoutSpec = new SellerPayoutByOrderIdSpecification(orderId);
            var payouts = await _unitOfWork.GetRepository<SellerPayout, int>()
                .GetAllAsync(payoutSpec);
            _logger.LogInformation(
                "OrderId={OrderId} has {PayoutCount} payouts before delivery processing.",
                orderId, payouts.Count());

            var processedPayoutCount = 0;
            foreach (var payout in payouts.Where(p =>
                p.Status == PayoutStatus.Pending ||
                p.Status == PayoutStatus.Processing))
            {
                var sellerProfile = await _unitOfWork.GetRepository<SellerProfile, int>()
                    .GetByIdAsync(payout.SellerProfileId);

                if (sellerProfile == null)
                    continue;

                sellerProfile.PendingCommission += payout.CommissionAmount;
                _logger.LogInformation(
                    "OrderId={OrderId}, SellerProfileId={SellerProfileId}: PendingCommission increased by {CommissionAmount}. New PendingCommission={PendingCommission}.",
                    orderId, sellerProfile.Id, payout.CommissionAmount, sellerProfile.PendingCommission);

                var commissionTx = new CommissionTransaction
                {
                    SellerProfileId = sellerProfile.Id,
                    OrderId = orderId,
                    OrderTotal = payout.OrderItemsTotal,
                    CommissionAmount = payout.CommissionAmount,
                    Type = "cash_debt",
                    Description = $"Cash order #{orderId} delivered - commission owed",
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.GetRepository<CommissionTransaction, int>()
                    .AddAsync(commissionTx);

                if (sellerProfile.PendingCommission >= sellerProfile.MaxAllowedCommission)
                {
                    sellerProfile.IsBlocked = true;
                    sellerProfile.BlockReason =
                        $"Pending commission exceeded {sellerProfile.MaxAllowedCommission} EGP. Please settle your balance.";
                    sellerProfile.BlockedAt = DateTime.UtcNow;
                }

                _unitOfWork.GetRepository<SellerProfile, int>().Update(sellerProfile);

                payout.Status = PayoutStatus.Completed;
                payout.ProcessedAt = DateTime.UtcNow;
                payout.PaidAt = DateTime.UtcNow;
                _unitOfWork.GetRepository<SellerPayout, int>().Update(payout);
                processedPayoutCount++;
            }

            var affected = await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation(
                "ConfirmCashPaymentAfterDeliveryAsync completed for OrderId={OrderId}. ProcessedPayouts={ProcessedPayouts}, DbRowsAffected={AffectedRows}.",
                orderId, processedPayoutCount, affected);
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

        private static void ValidateOrdersForPayment(IEnumerable<Order> orders)
        {
            foreach (var order in orders)
            {
                ValidateOrderForPayment(order);
            }
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

        private async Task<List<SellerPayout>> BuildSellerPayoutsAsync(List<Order> orders)
        {
            var payouts = new List<SellerPayout>();
            foreach (var order in orders)
            {
                var existingPayouts = await _unitOfWork.GetRepository<SellerPayout, int>()
                    .GetAllAsync(new SellerPayoutByOrderIdSpecification(order.Id));

                foreach (var group in order.OrderItems!.GroupBy(oi => oi.SellerId))
                {
                    var sellerProfile = await GetSellerProfileAsync(group.Key);

                    // Minimal safety guard: prevent duplicate payout creation
                    // for the same (OrderId + SellerId) pair.
                    if (existingPayouts.Any(p => p.SellerProfileId == sellerProfile.Id))
                        continue;

                    var itemsTotal = Math.Round(group.Sum(oi => oi.UnitPrice * oi.Quantity), 2);
                    var commission = Math.Round(itemsTotal * (sellerProfile.CommissionRate / 100m), 2);
                    var netAmount = Math.Round(itemsTotal - commission, 2);

                    payouts.Add(new SellerPayout
                    {
                        SellerProfileId = sellerProfile.Id,
                        OrderId = order.Id,
                        OrderItemsTotal = itemsTotal,
                        CommissionAmount = commission,
                        NetAmount = netAmount,
                        Status = PayoutStatus.Pending,
                        CreatedAt = DateTime.UtcNow
                    });
                }
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
            List<Order> orders,
            decimal totalAmount,
            string paymentToken,
            string paymobOrderId,
            string merchantOrderId)
        {

            if (existingPayment != null)
            {
                existingPayment.Method = PaymentMethod.Card;
                existingPayment.Status = PaymentStatus.Pending;
                existingPayment.PaidAt = null;
                existingPayment.PaymobTransactionId = paymentToken;
                existingPayment.PaymobOrderId = paymobOrderId;
                existingPayment.MerchantOrderId = merchantOrderId;
                existingPayment.Amount = totalAmount;

                _unitOfWork.GetRepository<Payment, int>().Update(existingPayment);
            }
            else
            {
                var payment = new Payment
                {
                    Amount = totalAmount,
                    Currency = "EGP",
                    Method = PaymentMethod.Card,
                    Status = PaymentStatus.Pending,
                    PaymobTransactionId = paymentToken,
                    PaymobOrderId = paymobOrderId,
                    MerchantOrderId = merchantOrderId,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.GetRepository<Payment, int>().AddAsync(payment);
                existingPayment = payment;
            }

            if (existingPayment != null)
            {
                foreach (var ord in orders)
                {
                    ord.PaymentId = existingPayment.Id;
                    _unitOfWork.GetRepository<Order, int>().Update(ord);
                }
            }
        }

        private async Task SavePayoutsAsync(
            List<int> orderIds,
            List<SellerPayout> newPayouts,
            string paymentToken)
        {
            var existingPayouts = new List<SellerPayout>();
            foreach (var oid in orderIds)
            {
                var existingPayoutsSpec = new SellerPayoutByOrderIdSpecification(oid);
                existingPayouts.AddRange(await _unitOfWork.GetRepository<SellerPayout, int>()
                    .GetAllAsync(existingPayoutsSpec));
            }

            if (!existingPayouts.Any())
            {
                foreach (var payout in newPayouts)
                {
                    payout.PaymobTransactionId = paymentToken;
                    await _unitOfWork.GetRepository<SellerPayout, int>().AddAsync(payout);
                }
                return;
            }

            foreach (var existing in existingPayouts
                         .Where(p => p.Status == PayoutStatus.Pending ||
                                     p.Status == PayoutStatus.Failed))
            {
                existing.PaymobTransactionId = paymentToken;
                _unitOfWork.GetRepository<SellerPayout, int>().Update(existing);
            }

            var existingSellerIds = existingPayouts
                .Select(p => p.SellerProfileId)
                .ToHashSet();

            foreach (var newPayout in newPayouts
                         .Where(p => !existingSellerIds.Contains(p.SellerProfileId)))
            {
                newPayout.PaymobTransactionId = paymentToken;
                await _unitOfWork.GetRepository<SellerPayout, int>().AddAsync(newPayout);
            }
        }

        private PaymentResponseDTO BuildPaymentResponse(int orderId, decimal totalAmount, string paymentToken)
        {
            var iframeId = _config["Paymob:IframeId"];
            var paymentUrl =
                $"https://accept.paymob.com/api/acceptance/iframes/{iframeId}?payment_token={paymentToken}";

            return new PaymentResponseDTO
            {
                PaymentUrl = paymentUrl,
                PaymentToken = paymentToken,
                OrderId = orderId,
                Amount = totalAmount
            };
        }
 

        private async Task CompletePaymentAsync(Payment payment, PaymobCallbackDTO callback)
        {
            payment.Status = PaymentStatus.Completed;
            payment.PaidAt = DateTime.UtcNow;
            payment.PaymobTransactionId = callback.id;
            _unitOfWork.GetRepository<Payment, int>().Update(payment);

            var paymentOrders = await GetPaymentOrdersByPaymentIdAsync(payment.Id);
            foreach (var order in paymentOrders)
            {
                order.Status = OrderStatus.Paid;
                _unitOfWork.GetRepository<Order, int>().Update(order);
            }

            foreach (var order in paymentOrders)
            {
                var payoutSpec = new SellerPayoutByOrderIdSpecification(order.Id);
                var payouts = await _unitOfWork.GetRepository<SellerPayout, int>()
                    .GetAllAsync(payoutSpec);

                foreach (var payout in payouts.Where(p => p.Status == PayoutStatus.Pending))
                {
                    payout.Status = PayoutStatus.Processing;
                    payout.PaymobTransactionId = callback.id;
                    _unitOfWork.GetRepository<SellerPayout, int>().Update(payout);
                }
            }
        }
 

        private async Task<(string PaymentKey, string PaymobOrderId)> CreatePaymobPaymentAsync(decimal totalAmount, List<OrderItem> items, Order buyerOrderContext, string merchantOrderId)
        {
            var authToken = await GetAuthTokenAsync();
            var paymobOrderId = await CreatePaymobOrderAsync(authToken, totalAmount, items, merchantOrderId);
            var paymentKey = await GetPaymentKeyAsync(authToken, paymobOrderId, totalAmount, buyerOrderContext);
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

        private async Task<int> CreatePaymobOrderAsync(string authToken, decimal totalAmount, List<OrderItem> items, string merchantOrderId)        {
            var request = new
            {
                auth_token = authToken,
                delivery_needed = false,
                amount_cents = (int)Math.Round(totalAmount * 100m),
                currency = "EGP",
                merchant_order_id = merchantOrderId,
                items = items.Select(oi => new
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

        private async Task<string> GetPaymentKeyAsync(string authToken, int paymobOrderId, decimal totalAmount, Order order)
        {
            var request = new
            {
                auth_token = authToken,
                amount_cents = (int)(totalAmount * 100),
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

        private bool VerifyHmac(PaymobCallbackDTO callback, string receivedHmac)
        {
            var hmacEnabled = _config["Paymob:HMAC_ENABLED"];
            if (hmacEnabled?.ToLower() == "false")
            {
                _logger.LogWarning(
                    "HMAC verification is disabled in configuration. " +
                    "Accepting callback without HMAC verification. " +
                    "OrderId={OrderId}, TransactionId={TransactionId}",
                    callback.order, callback.id);
                return true;
            }

            var hmacSecret = _config["Paymob:HmacSecret"];
            if (string.IsNullOrEmpty(hmacSecret))
            {
                _logger.LogError("HMAC verification failed: HmacSecret is not configured");
                return false;
            }

            if (string.IsNullOrEmpty(receivedHmac))
            {
                _logger.LogWarning("HMAC verification failed: received HMAC is empty");
                return false;
            }

            var dataString = string.Concat(
                callback.amount_cents,
                callback.created_at,
                callback.currency,
                callback.error_occured,
                callback.has_parent_transaction,
                callback.id,
                callback.integration_id,
                callback.is_captured,
                callback.is_refunded_transaction,
                callback.is_standalone_payment,
                callback.is_voided,
                callback.order,
                callback.owner,
                callback.pending,
                callback.source_data_pan,
                callback.source_data_sub_type,
                callback.source_data_type,
                callback.success);

            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(hmacSecret));
            var computedHmac = BitConverter.ToString(
                hmac.ComputeHash(Encoding.UTF8.GetBytes(dataString)))
                .Replace("-", "").ToLower();

            var isValid = computedHmac == receivedHmac.ToLower();

            if (!isValid)
            {
                _logger.LogWarning(
                    "HMAC verification failed. Expected: {Expected}, Received: {Received}, Data: {Data}",
                    computedHmac, receivedHmac, dataString);
            }

            return isValid;
        }
        
        
        private async Task<PaymentResponseDTO> HandleCashPaymentAsync(List<Order> orders, int orderId, Payment? existingPayment)
        {
            var sellerPayouts = await BuildSellerPayoutsAsync(orders);
            var existingOrderIds = new HashSet<int>();
            foreach (var ord in orders)
            {
                var spec = new SellerPayoutByOrderIdSpecification(ord.Id);
                var existing = await _unitOfWork.GetRepository<SellerPayout, int>()
                    .GetAllAsync(spec);
                if (existing.Any())
                    existingOrderIds.Add(ord.Id);
            }

            foreach (var payout in sellerPayouts.Where(p => !existingOrderIds.Contains(p.OrderId)))
            {
                payout.Status = PayoutStatus.Pending;
                await _unitOfWork.GetRepository<SellerPayout, int>().AddAsync(payout);
            }

            var payment = existingPayment ?? new Payment
            {
                Amount = orders.Sum(o => o.TotalPrice),
                Currency = "EGP",
                Method = PaymentMethod.Cash,
                Status = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };
            payment.Method = PaymentMethod.Cash;
            payment.Status = PaymentStatus.Pending;
            payment.Amount = orders.Sum(o => o.TotalPrice);

            if (existingPayment == null)
                await _unitOfWork.GetRepository<Payment, int>().AddAsync(payment);
            else
                _unitOfWork.GetRepository<Payment, int>().Update(payment);

            foreach (var ord in orders)
            {
                ord.Payment = payment;
                _unitOfWork.GetRepository<Order, int>().Update(ord);
            }

            await _unitOfWork.SaveChangesAsync();

            return new PaymentResponseDTO
            {
                PaymentUrl = null,
                PaymentToken = null,
                OrderId = orderId,
                Amount = orders.Sum(o => o.TotalPrice),
                Message = "Cash order recorded successfully. Payment will be completed after delivery."
            };
        }
        private async Task ValidateSellerNotBlockedAsync(List<Order> orders)
        {
            foreach (var group in orders.SelectMany(o => o.OrderItems!).GroupBy(oi => oi.SellerId))
            {
                var sellerProfile = await GetSellerProfileAsync(group.Key);
                if (sellerProfile.IsBlocked)
                    throw new InvalidOperationException(
                        "Unable to process payment. Some products are currently unavailable");

            }
        }
        
        private async Task ValidateCashExposureAsync(List<Order> orders)
        {
            var currentOrderIds = orders.Select(o => o.Id).ToHashSet();
            foreach (var group in orders.SelectMany(o => o.OrderItems!).GroupBy(oi => oi.SellerId))
            {
                var sellerProfile = await GetSellerProfileAsync(group.Key);

                var itemsTotal = Math.Round(group.Sum(oi => oi.UnitPrice * oi.Quantity), 2);
                var newCommission = Math.Round(itemsTotal * (sellerProfile.CommissionRate / 100m), 2);

                var reservedCashCommission = await GetReservedCashCommissionAsync(sellerProfile.Id, currentOrderIds);
                var currentExposure = sellerProfile.PendingCommission + reservedCashCommission;
                var projectedExposure = currentExposure + newCommission;

                var shouldReject =
                    currentExposure >= sellerProfile.MaxAllowedCommission ||
                    (currentExposure > 0 && projectedExposure > sellerProfile.MaxAllowedCommission);

                if (shouldReject)
                {
                    _logger.LogWarning(
                        "Cash exposure rejected for seller {SellerId} ({StoreName}). CurrentExposure={CurrentExposure}, NewCommission={NewCommission}, ProjectedExposure={ProjectedExposure}, Limit={Limit}, OrderId={OrderId}",
                        sellerProfile.Id,
                        sellerProfile.StoreName,
                        currentExposure,
                        newCommission,
                        projectedExposure,
                        sellerProfile.MaxAllowedCommission,
                        string.Join(",", currentOrderIds));

                    throw new InvalidOperationException(
                        "Cash payment is currently unavailable for one or more items in your order. Please choose card payment or try again later.");
                }
            }
        }

        private async Task<decimal> GetReservedCashCommissionAsync(
            int sellerProfileId,
            HashSet<int> currentOrderIds)
        {
            var spec = new SellerPayoutExposureSpecification(sellerProfileId);

            var payouts = await _unitOfWork
                .GetRepository<SellerPayout, int>()
                .GetAllAsync(spec);

            return payouts
                .Where(p =>
                    !currentOrderIds.Contains(p.OrderId) &&   
                    p.Order?.Payment != null &&
                    p.Order.Payment.Method == PaymentMethod.Cash &&
                    p.Order.Payment.Status == PaymentStatus.Pending &&
                    p.Order.Status != OrderStatus.Cancelled &&
                    p.Order.Status != OrderStatus.Declined &&
                    p.Order.Status != OrderStatus.Completed)
                .Sum(p => p.CommissionAmount);
        }

        private async Task<bool> HasMissingSellerPayoutsAsync(List<Order> orders)
        {
            foreach (var ord in orders)
            {
                var spec = new SellerPayoutByOrderIdSpecification(ord.Id);
                var payouts = await _unitOfWork.GetRepository<SellerPayout, int>()
                    .GetAllAsync(spec);

                if (!payouts.Any())
                    return true;
            }

            return false;
        }

        private async Task MarkPaymentAsFailedAsync(PaymobCallbackDTO callback)
        {
            Payment? payment = null;

            if (callback.order > 0)
            {
                payment = await GetPaymentByPaymobOrderIdAsync(callback.order.ToString());
            }

            if (payment == null && !string.IsNullOrWhiteSpace(callback.merchant_order_id))
            {
                payment = await GetPaymentByMerchantOrderIdStoredAsync(callback.merchant_order_id);
            }

            if (payment == null)
                return;

            payment.Status = PaymentStatus.Failed;
            _unitOfWork.GetRepository<Payment, int>().Update(payment);

            var orders = await GetPaymentOrdersByPaymentIdAsync(payment.Id);
            foreach (var order in orders)
            {
                _unitOfWork.GetRepository<Order, int>().Update(order);
            }

            await _unitOfWork.SaveChangesAsync();
        }

        private async Task<List<Order>> GetPaymentOrdersAsync(Order order, string userId)
        {
            if (order.PaymentId.HasValue)
            {
                return await GetPaymentOrdersByPaymentIdAsync(order.PaymentId.Value, userId);
            }

            return new List<Order> { order };
        }

        private async Task<List<Order>> GetPaymentOrdersByPaymentIdAsync(int paymentId, string? userId = null)
        {
            var spec = new OrdersByPaymentIdSpecification(paymentId, userId);
            var orders = await _unitOfWork.GetRepository<Order, int>().GetAllAsync(spec);
            return orders.ToList();
        }

        private async Task<List<int>> GetPaymentOrderIdsAsync(Payment payment)
        {
            if (payment.Orders != null && payment.Orders.Any())
                return payment.Orders.Select(o => o.Id).ToList();

            if (payment.OrderId.HasValue)
                return new List<int> { payment.OrderId.Value };

            var orders = await GetPaymentOrdersByPaymentIdAsync(payment.Id);
            return orders.Select(o => o.Id).ToList();
        }
    }
}
