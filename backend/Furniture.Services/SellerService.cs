using System.Net.Http.Json;
using AutoMapper;
using Furniture.Domain.InterfacesRepositories;
using Furniture.Domain.Models;
using Furniture.Domain.Models.Enum;
using Furniture.Services.Specifications;
using Furniture.Services.Specifications.Seller;
using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos.Seller;
using Microsoft.Extensions.Configuration;

namespace Furniture.Services.Implementations
{
    public class SellerService : ISellerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public SellerService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IHttpClientFactory httpClientFactory,
            IConfiguration config)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _httpClient = httpClientFactory.CreateClient("Paymob");
            _config = config;
        }

        // ============================================
        // 1. Seller يسجل بيانات متجره
        // ============================================
        public async Task<SellerProfileDTO> CreateSellerProfileAsync(
            string userId,
            CreateSellerProfileDTO dto)
        {
            // ============================================
            // Step 1: تحقق إن الـ User موجود
            // ============================================
            var user = await _unitOfWork.GetRepository<ApplicationUser, string>()
                .GetByIdAsync(userId);

            if (user == null)
                throw new InvalidOperationException("User not found");

            // ============================================
            // Step 2: تحقق إن الـ User مسجل كـ Seller
            // ============================================
            if (user.Role != Roles.seller)
                throw new InvalidOperationException("User is not a Seller");

            // ============================================
            // Step 3: تحقق إن مفيش Profile قبل كده
            // ============================================
            var existingSpec = new SellerProfileByUserIdSpecification(userId);
            var existing = await _unitOfWork.GetRepository<SellerProfile, int>()
                .GetByIdAsync(existingSpec);

            if (existing != null)
                throw new InvalidOperationException("Seller profile already exists");

            // ============================================
            // Step 4: إنشاء SellerProfile
            // ============================================
            var sellerProfile = new SellerProfile
            {
                UserId = userId,
                StoreName = dto.StoreName,
                StoreDescription = dto.StoreDescription,
                BankName = dto.BankName,
                BankAccountNumber = dto.BankAccountNumber,
                CommissionRate = 10m,    // 10% Default
                IsVerified = false,       // في انتظار موافقة Admin
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.GetRepository<SellerProfile, int>()
                .AddAsync(sellerProfile);
            await _unitOfWork.SaveChangesAsync();

            // ============================================
            // Step 5: رجوع الـ DTO
            // ============================================
            return new SellerProfileDTO
            {
                Id = sellerProfile.Id,
                StoreName = sellerProfile.StoreName,
                StoreDescription = sellerProfile.StoreDescription,
                BankName = sellerProfile.BankName,
                BankAccountNumber = sellerProfile.BankAccountNumber,
                CommissionRate = sellerProfile.CommissionRate,
                IsVerified = sellerProfile.IsVerified,
                CreatedAt = sellerProfile.CreatedAt
            };
        }

        // ============================================
        // 2. Seller يشوف بيانات متجره
        // ============================================
        public async Task<SellerProfileDTO?> GetMyProfileAsync(string userId)
        {
            var spec = new SellerProfileByUserIdSpecification(userId);
            var seller = await _unitOfWork.GetRepository<SellerProfile, int>()
                .GetByIdAsync(spec);

            if (seller == null)
                return null;

            return new SellerProfileDTO
            {
                Id = seller.Id,
                StoreName = seller.StoreName,
                StoreDescription = seller.StoreDescription,
                BankName = seller.BankName,
                BankAccountNumber = seller.BankAccountNumber,
                CommissionRate = seller.CommissionRate,
                IsVerified = seller.IsVerified,
                CreatedAt = seller.CreatedAt
            };
        }

        // ============================================
        // 3. Admin يوافق على Seller
        //    + ينشئ Sub-merchant في Paymob
        // ============================================
        public async Task<bool> VerifySellerAsync(int sellerId)
        {
            // ============================================
            // Step 1: جلب الـ Seller
            // ============================================
            var seller = await _unitOfWork.GetRepository<SellerProfile, int>()
                .GetByIdAsync(sellerId);

            if (seller == null)
                return false;

            // لو متقبل قبل كده
            if (seller.IsVerified)
                throw new InvalidOperationException("Seller is already verified");

            // ============================================
            // Step 2: إنشاء Sub-merchant في Paymob
            // ============================================
            // نجيب بيانات الـ User
            var user = await _unitOfWork.GetRepository<ApplicationUser, string>()
                .GetByIdAsync(seller.UserId);

            if (user == null)
                throw new InvalidOperationException("User not found");

            var subMerchantId = await CreateSubMerchantInPaymobAsync(
                seller, user);

            // ============================================
            // Step 3: حفظ الـ PaymobMerchantId في Database
            // ============================================
            seller.PaymobMerchantId = subMerchantId;
            seller.IsVerified = true;

            _unitOfWork.GetRepository<SellerProfile, int>().Update(seller);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        // ============================================
        // 4. Seller يشوف أرباحه
        // ============================================
        public async Task<SellerEarningsDTO> GetEarningsAsync(string userId)
        {
            // ============================================
            // Step 1: جلب الـ SellerProfile
            // ============================================
            var sellerSpec = new SellerProfileByUserIdSpecification(userId);
            var seller = await _unitOfWork.GetRepository<SellerProfile, int>()
                .GetByIdAsync(sellerSpec);

            if (seller == null)
                throw new InvalidOperationException("Seller profile not found");

            // ============================================
            // Step 2: جلب كل الـ Payouts
            // ============================================
            var payoutSpec = new SellerPayoutSpecification(seller.Id);
            var payouts = await _unitOfWork.GetRepository<SellerPayout, int>()
                .GetAllAsync(payoutSpec);

            var payoutsList = payouts.ToList();

            // ============================================
            // Step 3: حساب الإحصائيات
            // ============================================
            return new SellerEarningsDTO
            {
                // إجمالي المبيعات (قبل العمولة)
                TotalSales = payoutsList.Sum(p => p.OrderItemsTotal),

                // إجمالي العمولة اللي اتخصمت
                TotalCommission = payoutsList.Sum(p => p.CommissionAmount),

                // الصافي (بعد العمولة)
                NetEarnings = payoutsList.Sum(p => p.NetAmount),

                // المبالغ في الانتظار
                PendingAmount = payoutsList
                    .Where(p => p.Status == PayoutStatus.Pending ||
                                p.Status == PayoutStatus.Processing)
                    .Sum(p => p.NetAmount),

                // المبالغ اللي اتحولت
                PaidAmount = payoutsList
                    .Where(p => p.Status == PayoutStatus.Completed)
                    .Sum(p => p.NetAmount)
            };
        }

        // ============================================
        // 5. Admin يشوف كل الـ Sellers
        // ============================================
        public async Task<List<SellerProfileDTO>> GetAllSellersAsync()
        {
            var spec = new SellerProfileSpecification();
            var sellers = await _unitOfWork.GetRepository<SellerProfile, int>()
                .GetAllAsync(spec);

            return sellers.Select(s => new SellerProfileDTO
            {
                Id = s.Id,
                StoreName = s.StoreName,
                StoreDescription = s.StoreDescription,
                BankName = s.BankName,
                BankAccountNumber = s.BankAccountNumber,
                CommissionRate = s.CommissionRate,
                IsVerified = s.IsVerified,
                CreatedAt = s.CreatedAt
            }).ToList();
        }

        // ============================================
        // 6. Admin يشوف Sellers في انتظار الموافقة
        // ============================================
        public async Task<List<SellerProfileDTO>> GetPendingSellersAsync()
        {
            // isVerified = false → لسه مش متقبلين
            var spec = new SellerProfileSpecification(isVerified: false);
            var sellers = await _unitOfWork.GetRepository<SellerProfile, int>()
                .GetAllAsync(spec);

            return sellers.Select(s => new SellerProfileDTO
            {
                Id = s.Id,
                StoreName = s.StoreName,
                StoreDescription = s.StoreDescription,
                BankName = s.BankName,
                BankAccountNumber = s.BankAccountNumber,
                CommissionRate = s.CommissionRate,
                IsVerified = s.IsVerified,
                CreatedAt = s.CreatedAt
            }).ToList();
        }

        // ============================================
        // Private: إنشاء Sub-merchant في Paymob
        // ============================================
        private async Task<string> CreateSubMerchantInPaymobAsync(
            SellerProfile seller,
            ApplicationUser user)
        {
            // ============================================
            // Step 1: Auth Token
            // ============================================
            var apiKey = _config["Paymob:ApiKey"];

            var authResponse = await _httpClient.PostAsJsonAsync(
                "https://accept.paymob.com/api/auth/tokens",
                new { api_key = apiKey });

            authResponse.EnsureSuccessStatusCode();

            var authResult = await authResponse.Content
                .ReadFromJsonAsync<PaymobAuthResponse>();
            var authToken = authResult!.Token;

            // ============================================
            // Step 2: إنشاء Sub-merchant
            // ============================================
            var request = new
            {
                auth_token = authToken,
                merchant = new
                {
                    // بيانات الـ Seller
                    first_name = user.Name,
                    last_name = "Seller",
                    email = user.Email,
                    phone_number = user.PhoneNumber ?? "01000000000",

                    // بيانات البنك
                    bank_account = new
                    {
                        bank_name = seller.BankName,
                        account_number = seller.BankAccountNumber,
                        account_holder_name = user.Name
                    }
                }
            };

            var response = await _httpClient.PostAsJsonAsync(
                "https://accept.paymob.com/api/submerchants/",
                request);

            response.EnsureSuccessStatusCode();

            var result = await response.Content
                .ReadFromJsonAsync<SubMerchantResponse>();

            return result!.Id.ToString();
        }
    }

    // ============================================
    // Paymob Response Models
    // ============================================
    internal class PaymobAuthResponse
    {
        public string Token { get; set; } = null!;
    }

    internal class SubMerchantResponse
    {
        public int Id { get; set; }
        public string? Status { get; set; }
    }
}