using CommunityCar.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.TokenId).HasMaxLength(50).IsRequired();
        builder.Property(rt => rt.Token).HasMaxLength(500).IsRequired();
        builder.Property(rt => rt.DeviceFingerprint).HasMaxLength(200).IsRequired();
        builder.Property(rt => rt.CreatedByIp).HasMaxLength(45).IsRequired();
        builder.Property(rt => rt.RevokedByIp).HasMaxLength(45);

        // Relationships
        builder.HasOne(rt => rt.User)
              .WithMany()
              .HasForeignKey(rt => rt.UserId)
              .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(rt => rt.UserId);
        builder.HasIndex(rt => rt.Token).IsUnique();
        builder.HasIndex(rt => rt.TokenId).IsUnique();
        builder.HasIndex(rt => rt.ExpiresAt);
    }
}
