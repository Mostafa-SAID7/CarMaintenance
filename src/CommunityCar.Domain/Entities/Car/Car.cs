using CommunityCar.Domain.Common;
using CommunityCar.Domain.Entities.Auth;
using CommunityCar.Domain.Entities.Booking;

namespace CommunityCar.Domain.Entities.Car;

public class Car : BaseEntity
{
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public decimal DailyRate { get; set; }
    public bool IsAvailable { get; set; } = true;

    // Foreign key
    public string OwnerId { get; set; } = string.Empty;

    // Navigation properties
    public virtual User Owner { get; set; } = null!;
    public virtual ICollection<CommunityCar.Domain.Entities.Booking.Booking> Bookings { get; set; } = new List<CommunityCar.Domain.Entities.Booking.Booking>();
}
