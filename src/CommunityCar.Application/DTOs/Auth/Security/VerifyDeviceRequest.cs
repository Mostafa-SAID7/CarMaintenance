using System.ComponentModel.DataAnnotations;

namespace CommunityCar.Application.DTOs.Auth;

public class VerifyDeviceRequest
{
    [Required]
    public string DeviceId { get; set; } = string.Empty;

    [Required]
    public string VerificationCode { get; set; } = string.Empty; // OTP or similar
}