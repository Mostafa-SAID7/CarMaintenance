using CommunityCar.Domain.Common;
using CommunityCar.Domain.Entities.Auth;
using CommunityCar.Domain.Entities.Car;

namespace CommunityCar.Domain.Entities.Booking;

public class Booking : BaseEntity
{
    public string RenterId { get; set; } = string.Empty;
    public int CarId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Confirmed, Active, Completed, Cancelled
    public string? Notes { get; set; }

    // Navigation properties
    public virtual User Renter { get; set; } = null!;
    public virtual CommunityCar.Domain.Entities.Car.Car Car { get; set; } = null!;
}
