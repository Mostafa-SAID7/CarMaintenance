using CommunityCar.Domain.Entities.Profile;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations;

public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.HasKey(up => up.Id);
        builder.HasOne(up => up.User)
            .WithOne()
            .HasForeignKey<UserProfile>(up => up.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(up => up.DisplayName).HasMaxLength(100);
        builder.Property(up => up.Bio).HasMaxLength(500);
        builder.Property(up => up.PhoneNumber).HasMaxLength(50);
        builder.Property(up => up.Gender).HasMaxLength(20);
        builder.Property(up => up.Address).HasMaxLength(200);
        builder.Property(up => up.City).HasMaxLength(100);
        builder.Property(up => up.State).HasMaxLength(100);
        builder.Property(up => up.ZipCode).HasMaxLength(20);
        builder.Property(up => up.Country).HasMaxLength(100);
        builder.Property(up => up.Website).HasMaxLength(200);
        builder.Property(up => up.Occupation).HasMaxLength(100);
        builder.Property(up => up.Company).HasMaxLength(200);
        builder.Property(up => up.ProfilePictureUrl).HasMaxLength(500);
        builder.Property(up => up.CoverPhotoUrl).HasMaxLength(500);
        builder.Property(up => up.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        builder.Property(up => up.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
    }
}
