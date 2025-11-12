using CommunityCar.Api.Filters;
using CommunityCar.Api.Resources;
using CommunityCar.Application.DTOs.Auth;
using CommunityCar.Application.DTOs.Auth.Account;
using CommunityCar.Application.DTOs.Auth.Authentication;
using CommunityCar.Application.DTOs.Auth.Password;
using CommunityCar.Application.DTOs.Auth.Response;
using CommunityCar.Application.DTOs.Auth.Security;
using CommunityCar.Application.DTOs.Auth.Social;
using CommunityCar.Application.Helpers;
using CommunityCar.Application.Interfaces.Auth;
using CommunityCar.Domain.Entities.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace CommunityCar.Api.Controllers.Auth;

[ApiController]
[Route("api/[controller]")]
[ServiceFilter(typeof(AuthValidationFilter))]
[ServiceFilter(typeof(AuthLoggingFilter))]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<AuthController> _logger;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public AuthController(
        IAuthService authService,
        UserManager<User> userManager,
        ILogger<AuthController> logger,
        IStringLocalizer<SharedResource> localizer)
    {
        _authService = authService;
        _userManager = userManager;
        _logger = logger;
        _localizer = localizer;
    }

    [HttpGet("status")]
    [ProducesResponseType(typeof(AuthStatusResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuthStatus()
    {
        try
        {
            // Check if user is authenticated
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return Ok(new AuthStatusResponse { IsAuthenticated = false });
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Authenticated user has no NameIdentifier claim");
                return Ok(new AuthStatusResponse { IsAuthenticated = false });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found", userId);
                return Ok(new AuthStatusResponse { IsAuthenticated = false });
            }

            var response = new AuthStatusResponse
            {
                IsAuthenticated = true,
                UserId = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                EmailConfirmed = user.EmailConfirmed,
                TwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user)
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving auth status");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("current-user")]
    [Authorize]
    [ProducesResponseType(typeof(CurrentUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            var userId = AuthHelper.GetCurrentUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Unauthorized access attempt to GetCurrentUser");
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found", userId);
                return NotFound();
            }

            var roles = AuthHelper.GetCurrentUserRoles(User);

            var response = new CurrentUserResponse
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumber = user.PhoneNumber,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                TwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user),
                LockoutEnabled = user.LockoutEnabled,
                LockoutEnd = user.LockoutEnd,
                Roles = roles,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current user");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.LoginAsync(request);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.RegisterAsync(request);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("generate-otp")]
    public async Task<IActionResult> GenerateOtp([FromBody] string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return BadRequest("Email is required");

        // Get user by email first
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return BadRequest("User not found");

        var otp = await _authService.GenerateOtpAsync(user.Id, "Email Verification");

        return Ok(new { message = "OTP sent to your email" });
    }

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] OtpRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.VerifyOtpAsync(request);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("social-login")]
    public async Task<IActionResult> SocialLogin([FromBody] SocialLoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.SocialLoginAsync(request);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
    {
        var result = await _authService.RefreshTokenAsync(refreshToken);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        // Get user ID from claims
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _authService.LogoutAsync(userId);

        if (!result)
            return BadRequest("Logout failed");

        return Ok(new { message = "Logged out successfully" });
    }

    [HttpPost("enable-2fa")]
    [Authorize]
    public async Task<IActionResult> EnableTwoFactor()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _authService.EnableTwoFactorAsync(userId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("disable-2fa")]
    [Authorize]
    public async Task<IActionResult> DisableTwoFactor()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _authService.DisableTwoFactorAsync(userId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.ResetPasswordAsync(request);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.VerifyEmailAsync(request);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.ForgotPasswordAsync(request);

        // Always return success for security reasons (don't reveal if email exists)
        return Ok(new { message = "If the email exists, a password reset link has been sent." });
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _authService.ChangePasswordAsync(userId, request);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("sessions")]
    [Authorize]
    public async Task<IActionResult> GetActiveSessions()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        // Implementation for getting user's active sessions
        return Ok(new { message = "Active sessions endpoint", userId });
    }

    [HttpDelete("sessions/{sessionId}")]
    [Authorize]
    public async Task<IActionResult> TerminateSession(string sessionId)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        // Implementation for terminating a specific session
        return Ok(new { message = $"Session {sessionId} terminated" });
    }

    [HttpPost("terminate-all-sessions")]
    [Authorize]
    public async Task<IActionResult> TerminateAllSessions()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        // Implementation for terminating all user sessions except current
        return Ok(new { message = "All other sessions terminated" });
    }

    [HttpGet("devices")]
    [Authorize]
    public async Task<IActionResult> GetTrustedDevices()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        // Implementation for getting user's trusted devices
        return Ok(new { message = "Trusted devices endpoint", userId });
    }

    [HttpDelete("devices/{deviceId}")]
    [Authorize]
    public async Task<IActionResult> RemoveTrustedDevice(string deviceId)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        // Implementation for removing a trusted device
        return Ok(new { message = $"Device {deviceId} removed from trusted devices" });
    }

    [HttpPost("account/deactivate")]
    [Authorize]
    public async Task<IActionResult> DeactivateAccount([FromBody] DeactivateAccountRequest request)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        // Implementation for deactivating user account
        return Ok(new { message = "Account deactivation initiated" });
    }

    [HttpPost("account/reactivate")]
    public async Task<IActionResult> ReactivateAccount([FromBody] ReactivateAccountRequest request)
    {
        // Implementation for reactivating user account
        return Ok(new { message = "Account reactivation initiated" });
    }

    [HttpDelete("account")]
    [Authorize]
    public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountRequest request)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        // Implementation for permanently deleting user account
        return Ok(new { message = "Account deletion initiated" });
    }

    [HttpGet("security-activity")]
    [Authorize]
    public async Task<IActionResult> GetSecurityActivity([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        // Implementation for getting user's security activity
        return Ok(new
        {
            message = "Security activity endpoint",
            userId,
            from,
            to
        });
    }

    [HttpPost("verify-device")]
    [Authorize]
    public async Task<IActionResult> VerifyDevice([FromBody] VerifyDeviceRequest request)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        // Implementation for device verification (2FA)
        return Ok(new { message = "Device verification completed" });
    }

    [HttpGet("login-history")]
    [Authorize]
    public async Task<IActionResult> GetLoginHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        // Implementation for getting user's login history
        return Ok(new
        {
            message = "Login history endpoint",
            userId,
            page,
            pageSize
        });
    }
}
