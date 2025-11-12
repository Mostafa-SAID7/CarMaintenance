using CommunityCar.Domain.Entities.Booking;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.Property(b => b.TotalPrice).HasPrecision(18, 2);
        builder.Property(b => b.Status).HasMaxLength(20).IsRequired();
        builder.Property(b => b.Notes).HasMaxLength(1000);

        // Relationships
        builder.HasOne(b => b.Renter)
              .WithMany(u => u.Bookings)
              .HasForeignKey(b => b.RenterId)
              .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Car)
              .WithMany(c => c.Bookings)
              .HasForeignKey(b => b.CarId)
              .OnDelete(DeleteBehavior.Restrict);

        // Constraints
        builder.HasCheckConstraint("CK_Booking_EndDate", "EndDate > StartDate");
        builder.HasCheckConstraint("CK_Booking_Status", "Status IN ('Pending', 'Confirmed', 'Active', 'Completed', 'Cancelled')");

        // Indexes
        builder.HasIndex(b => b.RenterId);
        builder.HasIndex(b => b.CarId);
        builder.HasIndex(b => b.Status);
        builder.HasIndex(b => new { b.StartDate, b.EndDate });
    }
}
