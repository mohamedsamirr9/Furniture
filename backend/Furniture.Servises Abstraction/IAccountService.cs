using Furniture.shared.Dtos.AuthDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Servises_Abstraction
{
    public interface IAccountService
    {

        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);

        Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto refreshToken);
        Task RevokeRefreshTokenAsync(string refreshToken);

        Task SendOtpAsync(string email);
        Task VerifyOtpAsync(string email, string otp);

        Task ForgotPasswordAsync(string email);
        Task ResetPasswordAsync(ResetPasswordDto dto);

        Task<UserDto> GetCurrentUserAsync(string userId);

        Task<UserDto> UpdateProfileAsync(string userId, UpdateProfileDto dto);

        Task DeleteAccountAsync(string userId);
    }
}
