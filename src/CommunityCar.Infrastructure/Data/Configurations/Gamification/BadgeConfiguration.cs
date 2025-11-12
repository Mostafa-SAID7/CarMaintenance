using CommunityCar.Domain.Entities.Community;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations;

public class BadgeConfiguration : IEntityTypeConfiguration<Badge>
{
    public void Configure(EntityTypeBuilder<Badge> builder)
    {
        builder.Property(b => b.Name).HasMaxLength(100).IsRequired();
        builder.Property(b => b.Description).HasMaxLength(500);
        builder.Property(b => b.IconUrl).HasMaxLength(200);
        builder.Property(b => b.Criteria).HasMaxLength(1000);
        builder.Property(b => b.PointsValue).HasDefaultValue(0);

        builder.HasIndex(b => b.Name).IsUnique();
        builder.HasIndex(b => b.Type);
        builder.HasIndex(b => b.Rarity);
        builder.HasIndex(b => b.IsActive);
    }
}
