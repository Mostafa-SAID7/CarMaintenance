using CommunityCar.Application.DTOs.Auth.Profile;
using CommunityCar.Application.Interfaces.Auth;
using CommunityCar.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CommunityCar.Api.Controllers.Auth;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;
    private readonly ILogger<ProfileController> _logger;

    // Constants for error messages
    private const string UserNotAuthenticated = "User not authenticated";
    private const string ProfileNotFound = "Profile not found";
    private const string InvalidProfileData = "Invalid profile data";
    private const string NoFileUploaded = "No file uploaded";
    private const string InvalidFileType = "Invalid file type. Only image files are allowed.";
    private const string FileTooLarge = "File size exceeds the maximum allowed limit.";
    private const int MaxFileSize = 5 * 1024 * 1024; // 5MB

    public ProfileController(IProfileService profileService, ILogger<ProfileController> logger)
    {
        _profileService = profileService;
        _logger = logger;
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(UserNotAuthenticated);

            var profile = await _profileService.GetProfileAsync(userId);
            if (profile == null)
                return NotFound(ProfileNotFound);

            return Ok(profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving profile for user");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{userId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicProfile(string userId)
    {
        try
        {
            var profile = await _profileService.GetPublicProfileAsync(userId);
            if (profile == null)
                return NotFound("Profile not found or not public");

            return Ok(profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving public profile for user {UserId}", userId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateProfile([FromBody] UpdateProfileRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(UserNotAuthenticated);

            if (!await _profileService.ValidateProfileDataAsync(request))
                return BadRequest(InvalidProfileData);

            var profile = await _profileService.CreateProfileAsync(userId, request);
            return CreatedAtAction(nameof(GetMyProfile), profile);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating profile for user");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(UserNotAuthenticated);

            if (!await _profileService.ValidateProfileDataAsync(request))
                return BadRequest(InvalidProfileData);

            var profile = await _profileService.UpdateProfileAsync(userId, request);
            if (profile == null)
                return NotFound(ProfileNotFound);

            return Ok(profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile for user");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteProfile()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(UserNotAuthenticated);

            var result = await _profileService.DeleteProfileAsync(userId);
            if (!result)
                return NotFound(ProfileNotFound);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting profile for user");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("profile-picture")]
    public async Task<IActionResult> UploadProfilePicture(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest(NoFileUploaded);

            if (!IsValidImageFile(file))
                return BadRequest(InvalidFileType);

            if (file.Length > MaxFileSize)
                return BadRequest(FileTooLarge);

            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(UserNotAuthenticated);

            var imageUrl = await _profileService.UploadProfilePictureAsync(userId, file);
            return Ok(new { profilePictureUrl = imageUrl });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading profile picture for user");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("cover-photo")]
    public async Task<IActionResult> UploadCoverPhoto(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest(NoFileUploaded);

            if (!IsValidImageFile(file))
                return BadRequest(InvalidFileType);

            if (file.Length > MaxFileSize)
                return BadRequest(FileTooLarge);

            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(UserNotAuthenticated);

            var imageUrl = await _profileService.UploadCoverPhotoAsync(userId, file);
            return Ok(new { coverPhotoUrl = imageUrl });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading cover photo for user");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("profile-picture")]
    public async Task<IActionResult> DeleteProfilePicture()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(UserNotAuthenticated);

            var result = await _profileService.DeleteProfilePictureAsync(userId);
            if (!result)
                return NotFound("Profile picture not found");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting profile picture for user");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("cover-photo")]
    public async Task<IActionResult> DeleteCoverPhoto()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(UserNotAuthenticated);

            var result = await _profileService.DeleteCoverPhotoAsync(userId);
            if (!result)
                return NotFound("Cover photo not found");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting cover photo for user");
            return StatusCode(500, "Internal server error");
        }
    }

    private bool IsValidImageFile(IFormFile file)
    {
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        return allowedExtensions.Contains(extension);
    }
}