using CommunityCar.Application.DTOs.Auth.Authentication;
using FluentValidation;

namespace CommunityCar.Application.Validators.Auth;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long");

        RuleFor(x => x.DeviceFingerprint)
            .MaximumLength(500).WithMessage("Device fingerprint is too long")
            .When(x => !string.IsNullOrEmpty(x.DeviceFingerprint));

        RuleFor(x => x.OtpCode)
            .Length(6).WithMessage("OTP code must be 6 digits")
            .Matches(@"^\d{6}$").WithMessage("OTP code must contain only digits")
            .When(x => !string.IsNullOrEmpty(x.OtpCode));

        RuleFor(x => x.BiometricToken)
            .MaximumLength(1000).WithMessage("Biometric token is too long")
            .When(x => !string.IsNullOrEmpty(x.BiometricToken));
    }
}
