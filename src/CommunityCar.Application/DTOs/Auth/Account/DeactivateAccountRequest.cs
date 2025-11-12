using System.ComponentModel.DataAnnotations;

namespace CommunityCar.Application.DTOs.Auth;

public class DeactivateAccountRequest
{
    [Required]
    public string Password { get; set; } = string.Empty;

    public string? Reason { get; set; }
}