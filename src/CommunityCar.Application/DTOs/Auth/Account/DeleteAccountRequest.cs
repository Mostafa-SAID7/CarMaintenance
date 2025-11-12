using System.ComponentModel.DataAnnotations;

namespace CommunityCar.Application.DTOs.Auth;

public class DeleteAccountRequest
{
    [Required]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string Confirmation { get; set; } = string.Empty; // e.g., "DELETE"
}
