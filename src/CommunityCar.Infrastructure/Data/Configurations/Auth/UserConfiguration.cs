using CommunityCar.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations.Auth;

/// <summary>
/// Entity configuration for the User entity
/// </summary>
public class UserConfiguration : BaseEntityConfiguration<User>
{
    /// <summary>
    /// Configures the User entity
    /// </summary>
    /// <param name="builder">The entity type builder</param>
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // Configure base entity properties
        ConfigureBaseEntity(builder);

        // Configure required name properties
        ConfigureRequiredStringProperty(
            builder,
            u => u.FirstName,
            EntityConfigurationConstants.ShortStringLength,
            nameof(User.FirstName));

        ConfigureRequiredStringProperty(
            builder,
            u => u.LastName,
            EntityConfigurationConstants.ShortStringLength,
            nameof(User.LastName));

        // Configure optional security-related properties
        ConfigureOptionalStringProperty(
            builder,
            u => u.TwoFactorSecret,
            32); // 32 characters for TOTP secret

        ConfigureOptionalStringProperty(
            builder,
            u => u.DeviceFingerprint,
            256); // SHA-256 hash length

        ConfigureOptionalStringProperty(
            builder,
            u => u.VerificationToken,
            64); // UUID length

        ConfigureOptionalStringProperty(
            builder,
            u => u.PasswordResetToken,
            64); // UUID length

        ConfigureOptionalStringProperty(
            builder,
            u => u.OtpCode,
            6); // Standard OTP length

        ConfigureOptionalStringProperty(
            builder,
            u => u.BiometricKey,
            512); // Sufficient for biometric key storage

        // Configure social login properties
        ConfigureOptionalStringProperty(
            builder,
            u => u.SocialLoginProvider,
            EntityConfigurationConstants.ShortStringLength);

        ConfigureOptionalStringProperty(
            builder,
            u => u.SocialLoginId,
            256); // Sufficient for social login IDs

        // Configure composite index for name searches
        builder.HasIndex(u => new { u.FirstName, u.LastName })
            .HasDatabaseName(string.Format(
                EntityConfigurationConstants.IndexPattern,
                nameof(User),
                "Name"));

        // Configure unique index for email (inherited from Identity)
        builder.HasIndex(u => u.Email)
            .HasDatabaseName(string.Format(
                EntityConfigurationConstants.UniqueIndexPattern,
                nameof(User),
                "Email"))
            .IsUnique();

        // Configure additional indexes for common queries
        builder.HasIndex(u => u.CreatedAt)
            .HasDatabaseName(string.Format(
                EntityConfigurationConstants.IndexPattern,
                nameof(User),
                EntityConfigurationConstants.CreatedAtProperty));

        builder.HasIndex(u => u.LastLoginAt)
            .HasDatabaseName(string.Format(
                EntityConfigurationConstants.IndexPattern,
                nameof(User),
                "LastLoginAt"));
    }
}
