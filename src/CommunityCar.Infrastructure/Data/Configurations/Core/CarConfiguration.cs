using CommunityCar.Domain.Entities.Car;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations;

public class CarConfiguration : IEntityTypeConfiguration<Car>
{
    public void Configure(EntityTypeBuilder<Car> builder)
    {
        builder.Property(c => c.Make).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Model).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Color).HasMaxLength(50).IsRequired();
        builder.Property(c => c.LicensePlate).HasMaxLength(20).IsRequired();
        builder.Property(c => c.Description).HasMaxLength(500);
        builder.Property(c => c.ImageUrl).HasMaxLength(500);
        builder.Property(c => c.DailyRate).HasPrecision(18, 2);

        // Relationships
        builder.HasOne(c => c.Owner)
              .WithMany(u => u.Cars)
              .HasForeignKey(c => c.OwnerId)
              .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(c => c.LicensePlate).IsUnique();
        builder.HasIndex(c => c.OwnerId);
        builder.HasIndex(c => c.IsAvailable);
        builder.HasIndex(c => new { c.Make, c.Model });
    }
}
