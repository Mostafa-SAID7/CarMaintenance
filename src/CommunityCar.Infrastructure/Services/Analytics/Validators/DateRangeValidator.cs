using System;

namespace CommunityCar.Infrastructure.Services.Analytics.Validators;

/// <summary>
/// Validator for date range parameters.
/// </summary>
public static class DateRangeValidator
{
    /// <summary>
    /// Validates that the date range is valid.
    /// </summary>
    /// <param name="startDate">The start date.</param>
    /// <param name="endDate">The end date.</param>
    /// <exception cref="ArgumentException">Thrown when the date range is invalid.</exception>
    public static void Validate(DateTime startDate, DateTime endDate)
    {
        if (startDate > endDate)
        {
            throw new ArgumentException("Start date must be before or equal to end date", nameof(startDate));
        }

        if (startDate > DateTime.UtcNow)
        {
            throw new ArgumentException("Start date cannot be in the future", nameof(startDate));
        }

        var maxRange = TimeSpan.FromDays(365); // Maximum 1 year range
        if (endDate - startDate > maxRange)
        {
            throw new ArgumentException("Date range cannot exceed 365 days", nameof(endDate));
        }
    }
}
