using CommunityCar.Domain.Entities.Community;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations;

public class UserBadgeConfiguration : IEntityTypeConfiguration<UserBadge>
{
    public void Configure(EntityTypeBuilder<UserBadge> builder)
    {
        builder.Property(ub => ub.EarnedReason).HasMaxLength(500);

        builder.HasOne(ub => ub.User)
            .WithMany()
            .HasForeignKey(ub => ub.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ub => ub.Badge)
            .WithMany(b => b.UserBadges)
            .HasForeignKey(ub => ub.BadgeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(ub => new { ub.UserId, ub.BadgeId }).IsUnique();
        builder.HasIndex(ub => ub.UserId);
        builder.HasIndex(ub => ub.BadgeId);
        builder.HasIndex(ub => ub.EarnedAt);
        builder.HasIndex(ub => ub.IsDisplayed);
    }
}
