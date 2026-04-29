using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Furniture.Domain.InterfacesRepositories;
using Furniture.Domain.Models;
using Furniture.Domain.Models.Enum;
using Furniture.Services.Specifications;
using Furniture.Services.Specifications.Seller;
using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos.Payment;
using Furniture.shared.Dtos.Seller;
using Microsoft.Extensions.Configuration;

namespace Furniture.Services.Implementations
{
    public class SellerPaymentService : ISellerPaymentService
    {
        private const decimal DefaultCommissionRate = 6m;
        
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;

        public SellerPaymentService(
            IUnitOfWork unitOfWork,
            IConfiguration config,
            IHttpClientFactory httpClientFactory)
        {
            _unitOfWork = unitOfWork;
            _config = config;
            _httpClientFactory = httpClientFactory;
        }

        

        public async Task<SellerProfileDTO> CreateSellerProfileAsync(string userId, CreateSellerProfileDTO dto)
        {
            var user = await _unitOfWork.GetRepository<ApplicationUser, string>()
                .GetByIdAsync(userId);

            if (user == null)
                throw new InvalidOperationException("User not found");

            if (user.Role != Roles.seller)
                throw new InvalidOperationException("User is not a Seller");

            var existingProfile = await GetSellerProfileByUserIdAsync(userId);
            if (existingProfile != null)
                throw new InvalidOperationException("Seller profile already exists");

            var sellerProfile = new SellerProfile
            {
                UserId = userId,
                StoreName = dto.StoreName,
                StoreDescription = dto.StoreDescription,
                BankName = dto.BankName,
                BankAccountNumber = dto.BankAccountNumber,
                BankCode = dto.BankCode,
                NationalId = dto.NationalId,
                CommissionRate = DefaultCommissionRate,
                IsVerified = false,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.GetRepository<SellerProfile, int>().AddAsync(sellerProfile);
            await _unitOfWork.SaveChangesAsync();

            return MapToDTO(sellerProfile);
        }

        public async Task<SellerProfileDTO?> GetMyProfileAsync(string userId)
        {
            var sellerProfile = await GetSellerProfileByUserIdAsync(userId);
            return sellerProfile == null ? null : MapToDTO(sellerProfile);
        }

        public async Task<bool> VerifySellerAsync(int sellerId)
        {
            var seller = await _unitOfWork.GetRepository<SellerProfile, int>()
                .GetByIdAsync(sellerId);

            if (seller == null)
                return false;

            if (seller.IsVerified)
                throw new InvalidOperationException("Seller is already verified");

            seller.IsVerified = true;

            _unitOfWork.GetRepository<SellerProfile, int>().Update(seller);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<SellerEarningsDTO> GetEarningsAsync(string userId)
        {
            var seller = await GetSellerProfileByUserIdAsync(userId);

            if (seller == null)
                throw new InvalidOperationException("Seller profile not found");

            var payoutSpec = new SellerPayoutSpecification(seller.Id);
            var allPayouts = (await _unitOfWork.GetRepository<SellerPayout, int>()
                .GetAllAsync(payoutSpec)).ToList();

            var validPayouts = allPayouts
                .Where(p => p.Status != PayoutStatus.Cancelled)
                .ToList();

            return new SellerEarningsDTO
            {
                TotalSales = validPayouts.Sum(p => p.OrderItemsTotal),
                TotalCommission = validPayouts.Sum(p => p.CommissionAmount),
                NetEarnings = validPayouts.Sum(p => p.NetAmount),
                PendingAmount = validPayouts
                    .Where(p => p.Status == PayoutStatus.Pending || p.Status == PayoutStatus.Processing)
                    .Sum(p => p.NetAmount),
                PaidAmount = validPayouts
                    .Where(p => p.Status == PayoutStatus.Completed)
                    .Sum(p => p.NetAmount)
            };
        }

        public async Task<List<SellerPayoutDTO>> GetSellerPayoutsAsync(string userId)
        {
            var seller = await GetSellerProfileByUserIdAsync(userId);
            if (seller == null)
                throw new InvalidOperationException("Seller profile not found");

            var payoutSpec = new SellerPayoutSpecification(seller.Id);
            var payouts = await _unitOfWork.GetRepository<SellerPayout, int>()
                .GetAllAsync(payoutSpec);

            return payouts.Select(p => new SellerPayoutDTO
            {
                Id = p.Id,
                OrderId = p.OrderId,
                Amount = p.NetAmount,
                CommissionAmount = p.CommissionAmount,
                Status = p.Status.ToString(),
                CreatedAt = p.CreatedAt,
                PaidAt = p.PaidAt,
                TransactionId = p.PaymobTransactionId
            }).ToList();
        }

        public async Task<List<SellerProfileDTO>> GetAllSellersAsync()
        {
            var spec = new SellerProfileSpecification();
            var sellers = await _unitOfWork.GetRepository<SellerProfile, int>()
                .GetAllAsync(spec);

            return sellers.Select(MapToDTO).ToList();
        }

        public async Task<List<SellerProfileDTO>> GetPendingSellersAsync()
        {
            var spec = new SellerProfileSpecification(isVerified: false);
            var sellers = await _unitOfWork.GetRepository<SellerProfile, int>()
                .GetAllAsync(spec);

            return sellers.Select(MapToDTO).ToList();
        }

        public async Task<bool> RetryFailedPayoutAsync(int payoutId)
        {
            var payout = await _unitOfWork.GetRepository<SellerPayout, int>()
                .GetByIdAsync(payoutId);

            if (payout == null || payout.Status != PayoutStatus.Failed)
                return false;

            payout.Status = PayoutStatus.Processing;
            payout.FailureReason = null;

            _unitOfWork.GetRepository<SellerPayout, int>().Update(payout);
            await _unitOfWork.SaveChangesAsync();

            await SendPayoutToSellerAsync(payout);
            await _unitOfWork.SaveChangesAsync();

            return payout.Status == PayoutStatus.Completed;
        }

        public async Task ProcessPayoutsForOrderAsync(int orderId)
        {
            var payoutSpec = new SellerPayoutByOrderIdSpecification(orderId);
            var payouts = await _unitOfWork.GetRepository<SellerPayout, int>()
                .GetAllAsync(payoutSpec);

            var processingPayouts = payouts
                .Where(p => p.Status == PayoutStatus.Processing)
                .ToList();

            if (!processingPayouts.Any())
                return;

            foreach (var payout in processingPayouts)
                await SendPayoutToSellerAsync(payout);

            await _unitOfWork.SaveChangesAsync();
        }
        
        
        public async Task<SellerDebtDTO> GetSellerDebtAsync(string userId)
        {
            var seller = await GetSellerProfileByUserIdAsync(userId);
            if (seller == null)
                throw new InvalidOperationException("Seller profile not found");

            return new SellerDebtDTO
            {
                PendingCommission = seller.PendingCommission,
                MaxAllowedCommission = seller.MaxAllowedCommission,
                IsBlocked = seller.IsBlocked,
                BlockReason = seller.BlockReason,
                RemainingBeforeBlock =
                    Math.Max(0, seller.MaxAllowedCommission - seller.PendingCommission)
            };
        }
        
        public async Task<SellerExposureDTO> GetSellerExposureAsync(int sellerId)
        {
            var seller = await _unitOfWork.GetRepository<SellerProfile, int>()
                .GetByIdAsync(sellerId);

            if (seller == null)
                throw new InvalidOperationException("Seller profile not found");

            var spec = new SellerPayoutExposureSpecification(sellerId);

            var payouts = await _unitOfWork.GetRepository<SellerPayout, int>()
                .GetAllAsync(spec);

            var reservedCashCommission = payouts
                .Where(p =>
                    p.Order?.Payment != null &&
                    p.Order.Payment.Method == PaymentMethod.Cash &&
                    p.Order.Payment.Status == PaymentStatus.Pending &&
                    p.Order.Status != OrderStatus.Cancelled &&
                    p.Order.Status != OrderStatus.Declined &&
                    p.Order.Status != OrderStatus.Completed)
                .Sum(p => p.CommissionAmount);

            var currentExposure = seller.PendingCommission + reservedCashCommission;

            return new SellerExposureDTO
            {
                SellerId = seller.Id,
                StoreName = seller.StoreName,
                PendingCommission = seller.PendingCommission,
                ReservedCashCommission = reservedCashCommission,
                CurrentExposure = currentExposure,
                MaxAllowedCommission = seller.MaxAllowedCommission,
                IsBlocked = seller.IsBlocked,
                IsOverLimit = currentExposure >= seller.MaxAllowedCommission
            };
        }
        public async Task<bool> UnblockSellerAsync(int sellerId)
        {
            var seller = await _unitOfWork.GetRepository<SellerProfile, int>()
                .GetByIdAsync(sellerId);

            if (seller == null)
                return false;

            if (!seller.IsBlocked)
                throw new InvalidOperationException("Seller is not blocked");

            if (seller.PendingCommission > 0)
                throw new InvalidOperationException(
                    $"Seller still has {seller.PendingCommission} EGP pending. Cannot unblock until settled.");

            seller.IsBlocked = false;
            seller.BlockReason = null;
            seller.BlockedAt = null;

            _unitOfWork.GetRepository<SellerProfile, int>().Update(seller);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> SettleSellerDebtAsync(int sellerId, decimal amount)
        {
            var seller = await _unitOfWork.GetRepository<SellerProfile, int>()
                .GetByIdAsync(sellerId);

            if (seller == null)
                return false;

            if (amount <= 0)
                throw new InvalidOperationException("Amount must be greater than 0");

            if (amount > seller.PendingCommission)
                throw new InvalidOperationException(
                    $"Amount {amount} exceeds pending commission {seller.PendingCommission}");

            seller.PendingCommission -= amount;

            var commissionTx = new CommissionTransaction
            {
                SellerProfileId = seller.Id,
                OrderId = null,
                OrderTotal = null,
                CommissionAmount = amount,
                Type = "manual_settlement",
                Description = $"Manual settlement of {amount} EGP by admin",
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.GetRepository<CommissionTransaction, int>()
                .AddAsync(commissionTx);

            if (seller.PendingCommission <= 0)
            {
                seller.PendingCommission = 0;
                seller.IsBlocked = false;
                seller.BlockReason = null;
                seller.BlockedAt = null;
            }

            _unitOfWork.GetRepository<SellerProfile, int>().Update(seller);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        // ============================================================
        // Private 
        // ============================================================

        private async Task<SellerProfile?> GetSellerProfileByUserIdAsync(string userId)
        {
            var spec = new SellerProfileByUserIdSpecification(userId);
            return await _unitOfWork.GetRepository<SellerProfile, int>()
                .GetByIdAsync(spec);
        }

        private async Task SendPayoutToSellerAsync(SellerPayout payout)
        {
            var sellerProfile = await _unitOfWork.GetRepository<SellerProfile, int>()
                .GetByIdAsync(payout.SellerProfileId);

            if (sellerProfile == null)
            {
                MarkPayoutAsFailed(payout, "Seller profile not found");
                return;
            }

            if (string.IsNullOrWhiteSpace(sellerProfile.BankAccountNumber) ||
                string.IsNullOrWhiteSpace(sellerProfile.BankCode))
            {
                MarkPayoutAsFailed(payout, "Seller bank details incomplete");
                return;
            }

            if (!sellerProfile.IsVerified)
            {
                MarkPayoutAsFailed(payout, "Seller is not verified");
                return;
            }

            try
            {
                var accessToken = await GetPayoutsTokenAsync();
                var baseUrl = GetRequiredConfig("PaymobPayouts:BaseUrl");

                var request = new DisburseRequest
                {
                    issuer = "instant_bank",
                    amount = payout.NetAmount,
                    full_name = sellerProfile.StoreName,
                    bank_card_number = sellerProfile.BankAccountNumber,
                    bank_code = sellerProfile.BankCode,
                    bank_transaction_type = "cash_transfer",
                    client_reference = $"payout-{payout.Id}-{Guid.NewGuid():N}"
                };

                var httpClient = _httpClientFactory.CreateClient("PaymobPayouts");
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await httpClient.PostAsJsonAsync($"{baseUrl}disburse/", request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    MarkPayoutAsFailed(payout, $"HTTP {(int)response.StatusCode}: {errorContent}");
                    return;
                }

                var result = await response.Content.ReadFromJsonAsync<DisburseResponse>();

                if (result?.disbursement_status == "success")
                {
                    payout.Status = PayoutStatus.Completed;
                    payout.PayoutTransactionId = result.transaction_id;
                    payout.ProcessedAt = DateTime.UtcNow;
                }
                else if (result?.disbursement_status == "pending")
                {
                    payout.Status = PayoutStatus.Processing;
                    payout.PayoutTransactionId = result.transaction_id;
                    payout.ProcessedAt = DateTime.UtcNow;
                }
                else
                {
                    MarkPayoutAsFailed(payout, result?.status_description ?? "Unknown error");
                }
            }
            catch (HttpRequestException ex)
            {
                MarkPayoutAsFailed(payout, $"Network error: {ex.Message}");
            }
            catch (TaskCanceledException ex)
            {
                MarkPayoutAsFailed(payout, $"Request timeout: {ex.Message}");
            }
            catch (Exception ex)
            {
                MarkPayoutAsFailed(payout, ex.Message);
            }

            _unitOfWork.GetRepository<SellerPayout, int>().Update(payout);
        }

        private async Task<string> GetPayoutsTokenAsync()
        {
            var baseUrl = GetRequiredConfig("PaymobPayouts:BaseUrl");
            var clientId = GetRequiredConfig("PaymobPayouts:ClientId");
            var clientSecret = GetRequiredConfig("PaymobPayouts:ClientSecret");
            var username = GetRequiredConfig("PaymobPayouts:Username");
            var password = GetRequiredConfig("PaymobPayouts:Password");

            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", username),
                new KeyValuePair<string, string>("password", password)
            });

            var credentials = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));

            var tokenClient = _httpClientFactory.CreateClient("PaymobPayoutsAuth");
            tokenClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", credentials);

            var response = await tokenClient.PostAsync($"{baseUrl}o/token/", formContent);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<PayoutsTokenResponse>();

            if (result == null || string.IsNullOrWhiteSpace(result.access_token))
                throw new InvalidOperationException("Failed to get Paymob Payouts token");

            return result.access_token;
        }

        private string GetRequiredConfig(string key)
        {
            var value = _config[key];

            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException($"{key} is not configured");

            return value;
        }

        private void MarkPayoutAsFailed(SellerPayout payout, string reason)
        {
            payout.Status = PayoutStatus.Failed;
            payout.FailureReason = reason;
            _unitOfWork.GetRepository<SellerPayout, int>().Update(payout);
        }

        private static SellerProfileDTO MapToDTO(SellerProfile sellerProfile) => new()
        {
            Id = sellerProfile.Id,
            StoreName = sellerProfile.StoreName,
            StoreDescription = sellerProfile.StoreDescription,
            BankName = sellerProfile.BankName,
            BankAccountNumber = sellerProfile.BankAccountNumber,
            BankCode = sellerProfile.BankCode,
            CommissionRate = sellerProfile.CommissionRate,
            IsVerified = sellerProfile.IsVerified,
            CreatedAt = sellerProfile.CreatedAt
        };
    }
}