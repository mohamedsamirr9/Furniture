using Furniture.presentation.Controllers;
using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos.AuthDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Furniture.web
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController(IAccountService _accountService) : ControllerBase
    {
       
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
        {
            var result = await _accountService.RegisterAsync(dto);
            return Ok(result);
        }

       
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
        {
            var result = await _accountService.LoginAsync(dto);
            return Ok(result);
        }

       
        [HttpPost("refresh")]
        public async Task<ActionResult<AuthResponseDto>> Refresh(RefreshTokenDto refreshToken)
        {
            var result = await _accountService.RefreshTokenAsync(refreshToken);
            return Ok(result);
        }

       
        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke(string refreshToken)
        {
            await _accountService.RevokeRefreshTokenAsync(refreshToken);
            return NoContent();
        }

       
        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp(SendOtpDto dto)
        {
            await _accountService.SendOtpAsync(dto.Email);
            return NoContent();
        }

       
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp(VerifyOtpDto dto)
        {
            await _accountService.VerifyOtpAsync(dto.Email, dto.Otp);
            return NoContent();
        }

      
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgetPasswordDto dto)
        {
            await _accountService.ForgotPasswordAsync(dto.Email);
            return NoContent();
        }

        
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            await _accountService.ResetPasswordAsync(dto);
            return NoContent();
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var user = await _accountService.GetCurrentUserAsync(userId!);
            return Ok(user);
        }

        [Authorize]
        [HttpPut("profile")]
        public async Task<ActionResult<UserDto>> UpdateProfile(UpdateProfileDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var user = await _accountService.UpdateProfileAsync(userId!, dto);
            return Ok(user);
        }


        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> DeleteAccount()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            await _accountService.DeleteAccountAsync(userId!);
            return NoContent();
        }

        [Authorize]
        [HttpPost("become-seller")]
        public async Task<IActionResult> BecomeSeller(BecomeSellerDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await _accountService.BecomeSellerAsync(userId!, dto);
return Ok(new { message = "Now you are a seller" });  
      }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await _accountService.ChangePasswordAync(userId!, dto);
            return Ok(new { message = "Password changed successfully" });
        }

        [HttpGet("admin/users")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var result = await _accountService.GetAllUsersAsync();
            return Ok(result);
        }
    }
}
