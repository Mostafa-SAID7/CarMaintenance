using CommunityCar.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations;

public class ApiKeyConfiguration : IEntityTypeConfiguration<ApiKey>
{
    public void Configure(EntityTypeBuilder<ApiKey> builder)
    {
        builder.HasKey(ak => ak.Id);
        builder.Property(ak => ak.Key).HasMaxLength(256).IsRequired();
        builder.Property(ak => ak.Name).HasMaxLength(100).IsRequired();
        builder.Property(ak => ak.Description).HasMaxLength(500);
        builder.Property(ak => ak.CreatedByIp).HasMaxLength(45);
        builder.Property(ak => ak.LastUsedByIp).HasMaxLength(45);

        builder.HasOne(ak => ak.User)
            .WithMany()
            .HasForeignKey(ak => ak.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(ak => ak.Key).IsUnique();
        builder.HasIndex(ak => ak.UserId);
        builder.HasIndex(ak => ak.ExpiresAt);
        builder.HasIndex(ak => ak.IsActive);
    }
}
