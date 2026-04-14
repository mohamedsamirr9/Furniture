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

        public SellerService(IUnitOfWork unitOfWork, IMapper mapper, IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _httpClient = httpClientFactory.CreateClient("Paymob");
            _config = config;
        }

        
        public async Task<SellerProfileDTO> CreateSellerProfileAsync(string userId, CreateSellerProfileDTO dto)
        {
            
            var user = await _unitOfWork.GetRepository<ApplicationUser, string>()
                .GetByIdAsync(userId);

            if (user == null)
                throw new InvalidOperationException("User not found");
            
            if (user.Role != Roles.seller)
                throw new InvalidOperationException("User is not a Seller");

            var existingSpec = new SellerProfileByUserIdSpecification(userId);
            var existing = await _unitOfWork.GetRepository<SellerProfile, int>()
                .GetByIdAsync(existingSpec);

            if (existing != null)
                throw new InvalidOperationException("Seller profile already exists");
            
            var sellerProfile = new SellerProfile
            {
                UserId = userId,
                StoreName = dto.StoreName,
                StoreDescription = dto.StoreDescription,
                BankName = dto.BankName,
                BankAccountNumber = dto.BankAccountNumber,
                CommissionRate = 10m,    
                IsVerified = false,       
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.GetRepository<SellerProfile, int>()
                .AddAsync(sellerProfile);
            await _unitOfWork.SaveChangesAsync();

           
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

       
        public async Task<bool> VerifySellerAsync(int sellerId)
        {
            
            var seller = await _unitOfWork.GetRepository<SellerProfile, int>()
                .GetByIdAsync(sellerId);

            if (seller == null)
                return false;

            if (seller.IsVerified)
                throw new InvalidOperationException("Seller is already verified");
            
            var user = await _unitOfWork.GetRepository<ApplicationUser, string>()
                .GetByIdAsync(seller.UserId);

            if (user == null)
                throw new InvalidOperationException("User not found");

            var subMerchantId = await CreateSubMerchantInPaymobAsync(
                seller, user);

            
            seller.PaymobMerchantId = subMerchantId;
            seller.IsVerified = true;

            _unitOfWork.GetRepository<SellerProfile, int>().Update(seller);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

       
        public async Task<SellerEarningsDTO> GetEarningsAsync(string userId)
        {
            
            var sellerSpec = new SellerProfileByUserIdSpecification(userId);
            var seller = await _unitOfWork.GetRepository<SellerProfile, int>()
                .GetByIdAsync(sellerSpec);

            if (seller == null)
                throw new InvalidOperationException("Seller profile not found");

           
            var payoutSpec = new SellerPayoutSpecification(seller.Id);
            var payouts = await _unitOfWork.GetRepository<SellerPayout, int>()
                .GetAllAsync(payoutSpec);

            var payoutsList = payouts.ToList();

            
            return new SellerEarningsDTO
            {
                TotalSales = payoutsList.Sum(p => p.OrderItemsTotal),
                TotalCommission = payoutsList.Sum(p => p.CommissionAmount),
                NetEarnings = payoutsList.Sum(p => p.NetAmount),
                PendingAmount = payoutsList
                    .Where(p => p.Status == PayoutStatus.Pending ||
                                p.Status == PayoutStatus.Processing)
                    .Sum(p => p.NetAmount),

                PaidAmount = payoutsList
                    .Where(p => p.Status == PayoutStatus.Completed)
                    .Sum(p => p.NetAmount)
            };
        }

       
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

        
        public async Task<List<SellerProfileDTO>> GetPendingSellersAsync()
        {
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

        
        private async Task<string> CreateSubMerchantInPaymobAsync(
            SellerProfile seller,
            ApplicationUser user)
        {
            var apiKey = _config["Paymob:ApiKey"];

            var authResponse = await _httpClient.PostAsJsonAsync(
                "https://accept.paymob.com/api/auth/tokens",
                new { api_key = apiKey });

            authResponse.EnsureSuccessStatusCode();

            var authResult = await authResponse.Content
                .ReadFromJsonAsync<PaymobAuthResponse>();
            var authToken = authResult!.Token;

            
            var request = new
            {
                auth_token = authToken,
                merchant = new
                {
                    first_name = user.Name,
                    last_name = "Seller",
                    email = user.Email,
                    phone_number = user.PhoneNumber ?? "01000000000",

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

   

    internal class SubMerchantResponse
    {
        public int Id { get; set; }
        public string? Status { get; set; }
    }
}