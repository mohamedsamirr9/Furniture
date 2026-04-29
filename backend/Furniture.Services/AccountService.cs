using AutoMapper;
using Furniture.Domain.InterfacesRepositories;
using Furniture.Domain.Models;
using Furniture.Domain.Models.Enum;
using Furniture.Services.Specifications;
using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos.AuthDto;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Services
{
    public class AccountService(
     UserManager<ApplicationUser> _userManager,
     IUnitOfWork _unitOfWork,
     IMapper _mapper,
     JwtHelper _jwtHelper,
     IEmailService _emailService) : IAccountService
    {
        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            var user = _mapper.Map<ApplicationUser>(dto);
            user.Role = Roles.buyer;
            user.IsVerified = false;

            if (!string.IsNullOrEmpty(dto.NationalIdImageBase64))
                user.NationalIdImage = ImageHelper.SaveImage(dto.NationalIdImageBase64);

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
            return await GenerateAuthResponse(user);
        }

       
        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user is null || !await _userManager.CheckPasswordAsync(user, dto.Password))
                throw new Exception("Invalid credentials");

            return await GenerateAuthResponse(user);
        }

        
        private async Task<AuthResponseDto> GenerateAuthResponse(ApplicationUser user)
        {
            var token = _jwtHelper.GenerateToken(user);

            var repo = _unitOfWork.GetRepository<RefrashToken, int>();

            var refreshToken = new RefrashToken
            {
                Token = Guid.NewGuid().ToString(),
                Expires = DateTime.UtcNow.AddDays(7),
                UserId = user.Id
            };

            await repo.AddAsync(refreshToken);
            await _unitOfWork.SaveChangesAsync();

            return new AuthResponseDto
            {
                Token = token,
                RefreshToken = refreshToken.Token,
                User = _mapper.Map<UserDto>(user)
            };
        }


        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
        {
            var repo = _unitOfWork.GetRepository<RefrashToken, int>();

            var token = (await repo.GetAllAsync())
                .FirstOrDefault(x => x.Token == refreshTokenDto.Token && !x.IsRevoked);

            if (token is null || token.Expires < DateTime.UtcNow)
                throw new Exception("Invalid refresh token");

            var user = await _userManager.FindByIdAsync(token.UserId);

            return await GenerateAuthResponse(user!);
        }


        public async Task RevokeRefreshTokenAsync(string refreshToken)
        {
            var repo = _unitOfWork.GetRepository<RefrashToken, int>();

            var token = (await repo.GetAllAsync())
                .FirstOrDefault(x => x.Token == refreshToken);

            if (token is null)
                throw new Exception("Token not found");

            token.IsRevoked = true;

            repo.Update(token);
            await _unitOfWork.SaveChangesAsync();
        }

       
        public async Task SendOtpAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user is null)
                throw new Exception("User not found");

            user.OTP = new Random().Next(100000, 999999).ToString();
            user.OTPExpiry = DateTime.UtcNow.AddMinutes(10);

            await _userManager.UpdateAsync(user);

            await _emailService.SendEmailAsync(
                email,
                "OTP Code",
                $"Your OTP is: {user.OTP}"
            );
        }

      
        public async Task VerifyOtpAsync(string email, string otp)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user is null ||
                user.OTP != otp ||
                user.OTPExpiry < DateTime.UtcNow)
            {
                throw new Exception("Invalid or expired OTP");
            }

            user.OTP = null;
            user.OTPExpiry = null;

            await _userManager.UpdateAsync(user);
        }

       
        public async Task ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user is null) return;

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encoded= WebUtility.UrlEncode(token);
            await _emailService.SendEmailAsync(
                email,
                "Reset Password",
                $"Token: {encoded}"
            );
        }

       
        public async Task ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user is null)
                throw new Exception("User not found");
            var decoded = WebUtility.UrlDecode(dto.Token);
            var result = await _userManager.ResetPasswordAsync(
                user,
                decoded,
                dto.NewPassword
            );

            if (!result.Succeeded)
                throw new Exception("Reset failed");
        }

       
        public async Task<UserDto> GetCurrentUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
                throw new Exception("User not found");

            return _mapper.Map<UserDto>(user);
        }

     
        public async Task<UserDto> UpdateProfileAsync(string userId, UpdateProfileDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
                throw new Exception("User not found");

            user.Name = dto.Name;
            user.Address = dto.Address;

            if (!string.IsNullOrEmpty(dto.ProfileImageBase64))
                user.ProfileImage = ImageHelper.SaveImage(dto.ProfileImageBase64);

            await _userManager.UpdateAsync(user);

            return _mapper.Map<UserDto>(user);
        }

      
        public async Task DeleteAccountAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
                throw new Exception("User not found");

            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;

            await _userManager.UpdateAsync(user);
        }

        public async Task BecomeSellerAsync(string UserId, BecomeSellerDto dto)
        {
            var user = await _userManager.FindByIdAsync(UserId);
            if (user is null)
                throw new Exception("user not found");
            if (user.Role == Roles.seller)
                throw new Exception("Already a seller");
            if (!string.IsNullOrEmpty(dto.NationalIdImageBase64))
                user.NationalIdImage = ImageHelper.SaveImage(dto.NationalIdImageBase64);

            user.Role = Roles.seller;
            user.IsVerified = false;
            await _userManager.UpdateAsync(user);

            var profileRepo = _unitOfWork.GetRepository<SellerProfile, int>();
            var existingProfile = await profileRepo.GetByIdAsync(new SellerProfileByUserIdSpecification(UserId));
            if (existingProfile is null)
            {
                var sellerProfile = new SellerProfile
                {
                    UserId = UserId,
                    StoreName = dto.StoreName,
                    StoreDescription = null,
                    CommissionRate = 6m,
                    IsVerified = false,
                    CreatedAt = DateTime.UtcNow
                };
                await profileRepo.AddAsync(sellerProfile);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task ChangePasswordAync(string UserId, ChangePasswordDto dto)
        {
            var user = await _userManager.FindByIdAsync(UserId);
            if (user is null)
                throw new Exception("User not found");
            var result= await _userManager.ChangePasswordAsync(user,dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded)
            {
                var errors= string.Join(",", result.Errors.Select(e=>e.Description));
                throw new Exception(errors);
            }
        }

        public Task<IEnumerable<AdminUserDto>> GetAllUsersAsync()
        {
            var users = _userManager.Users
                .OrderByDescending(u => u.RegisteredAt)
                .Select(u => new AdminUserDto
                {
                    Id            = u.Id,
                    Name          = u.Name,
                    Email         = u.Email ?? "",
                    Phone         = u.PhoneNumber,
                    Address       = u.Address,
                    Role          = u.Role.ToString(),
                    JoinDate      = u.RegisteredAt,
                    AccountStatus = u.IsDeleted ? "Deleted" : "Active",
                })
                .AsEnumerable();

            return Task.FromResult(users);
        }
    }
}

