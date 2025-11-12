using CommunityCar.Application.DTOs.Auth;
using CommunityCar.Application.DTOs.Auth.Authentication;
using CommunityCar.Application.DTOs.Auth.Password;
using CommunityCar.Application.DTOs.Auth.Security;
using CommunityCar.Application.DTOs.Auth.Social;

namespace CommunityCar.Application.Interfaces.Auth;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);
    Task<bool> LogoutAsync(string userId);
    Task<AuthResponse> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<AuthResponse> ResetPasswordAsync(ResetPasswordRequest request);
    Task<AuthResponse> VerifyEmailAsync(VerifyEmailRequest request);
    Task<AuthResponse> ResendEmailVerificationAsync(string email);
    Task<string> GenerateOtpAsync(string userId, string purpose);
    Task<AuthResponse> VerifyOtpAsync(OtpRequest request);
    Task<AuthResponse> SocialLoginAsync(SocialLoginRequest request);
    Task<AuthResponse> EnableTwoFactorAsync(string userId);
    Task<AuthResponse> DisableTwoFactorAsync(string userId);
    Task<AuthResponse> VerifyTwoFactorAsync(string userId, string code);
    Task<AuthResponse> ChangePasswordAsync(string userId, ChangePasswordRequest request);
    Task<bool> IsAccountLockedAsync(string userId);
    Task<bool> LogSecurityEventAsync(string userId, string action, string ipAddress, string userAgent);
}
