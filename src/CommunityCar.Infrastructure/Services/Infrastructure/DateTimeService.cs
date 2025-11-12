using CommunityCar.Application.Interfaces;

namespace CommunityCar.Infrastructure.Services.Infrastructure;

public class DateTimeService : IDateTimeService
{
    private readonly TimeZoneInfo _defaultTimeZone;

    public DateTimeService(string defaultTimeZoneId = "UTC")
    {
        _defaultTimeZone = TimeZoneInfo.FindSystemTimeZoneById(defaultTimeZoneId);
    }

    public DateTime Now => DateTime.Now;
    public DateTime UtcNow => DateTime.UtcNow;
    public DateTime Today => DateTime.Today;
    public DateTimeOffset OffsetNow => DateTimeOffset.Now;
    public DateTimeOffset OffsetUtcNow => DateTimeOffset.UtcNow;

    public DateTime ConvertToTimeZone(DateTime dateTime, string timeZoneId)
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        return TimeZoneInfo.ConvertTime(dateTime, timeZone);
    }

    public DateTime ConvertFromUtcToTimeZone(DateTime utcDateTime, string timeZoneId)
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, timeZone);
    }

    public DateTime ConvertToUtcFromTimeZone(DateTime localDateTime, string timeZoneId)
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        return TimeZoneInfo.ConvertTimeToUtc(localDateTime, timeZone);
    }

    public bool IsBusinessDay(DateTime date)
    {
        return date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday;
    }

    public DateTime GetNextBusinessDay(DateTime date)
    {
        var nextDay = date.AddDays(1);
        while (!IsBusinessDay(nextDay))
        {
            nextDay = nextDay.AddDays(1);
        }
        return nextDay;
    }

    public DateTime GetPreviousBusinessDay(DateTime date)
    {
        var previousDay = date.AddDays(-1);
        while (!IsBusinessDay(previousDay))
        {
            previousDay = previousDay.AddDays(-1);
        }
        return previousDay;
    }

    public int GetBusinessDaysBetween(DateTime startDate, DateTime endDate)
    {
        var businessDays = 0;
        var currentDate = startDate.Date;

        while (currentDate <= endDate.Date)
        {
            if (IsBusinessDay(currentDate))
            {
                businessDays++;
            }
            currentDate = currentDate.AddDays(1);
        }

        return businessDays;
    }

    public DateTime GetStartOfWeek(DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-1 * diff).Date;
    }

    public DateTime GetEndOfWeek(DateTime date)
    {
        return GetStartOfWeek(date).AddDays(6).Date.AddDays(1).AddTicks(-1);
    }

    public DateTime GetStartOfMonth(DateTime date)
    {
        return new DateTime(date.Year, date.Month, 1);
    }

    public DateTime GetEndOfMonth(DateTime date)
    {
        return new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
    }

    public DateTime GetStartOfQuarter(DateTime date)
    {
        var quarter = (date.Month - 1) / 3 + 1;
        var startMonth = (quarter - 1) * 3 + 1;
        return new DateTime(date.Year, startMonth, 1);
    }

    public DateTime GetEndOfQuarter(DateTime date)
    {
        var quarter = (date.Month - 1) / 3 + 1;
        var endMonth = quarter * 3;
        return new DateTime(date.Year, endMonth, DateTime.DaysInMonth(date.Year, endMonth));
    }

    public DateTime GetStartOfYear(DateTime date)
    {
        return new DateTime(date.Year, 1, 1);
    }

    public DateTime GetEndOfYear(DateTime date)
    {
        return new DateTime(date.Year, 12, 31);
    }

    public int GetAge(DateTime birthDate)
    {
        return GetAge(birthDate, DateTime.Today);
    }

    public int GetAge(DateTime birthDate, DateTime atDate)
    {
        var age = atDate.Year - birthDate.Year;
        if (atDate < birthDate.AddYears(age))
        {
            age--;
        }
        return age;
    }

    public TimeSpan GetDuration(DateTime startTime, DateTime endTime)
    {
        return endTime - startTime;
    }

    public string GetHumanReadableDuration(TimeSpan duration)
    {
        if (duration.TotalDays >= 365)
        {
            var years = (int)(duration.TotalDays / 365);
            return $"{years} year{(years != 1 ? "s" : "")}";
        }
        if (duration.TotalDays >= 30)
        {
            var months = (int)(duration.TotalDays / 30);
            return $"{months} month{(months != 1 ? "s" : "")}";
        }
        if (duration.TotalDays >= 1)
        {
            var days = (int)duration.TotalDays;
            return $"{days} day{(days != 1 ? "s" : "")}";
        }
        if (duration.TotalHours >= 1)
        {
            var hours = (int)duration.TotalHours;
            return $"{hours} hour{(hours != 1 ? "s" : "")}";
        }
        if (duration.TotalMinutes >= 1)
        {
            var minutes = (int)duration.TotalMinutes;
            return $"{minutes} minute{(minutes != 1 ? "s" : "")}";
        }
        var seconds = (int)duration.TotalSeconds;
        return $"{seconds} second{(seconds != 1 ? "s" : "")}";
    }

    public string GetTimeAgo(DateTime pastDateTime)
    {
        var duration = DateTime.UtcNow - pastDateTime;
        return GetHumanReadableDuration(duration) + " ago";
    }

    public string GetTimeUntil(DateTime futureDateTime)
    {
        var duration = futureDateTime - DateTime.UtcNow;
        return "in " + GetHumanReadableDuration(duration);
    }

    public bool IsValidDate(string dateString)
    {
        return DateTime.TryParse(dateString, out _);
    }

    public bool IsValidTime(string timeString)
    {
        return TimeSpan.TryParse(timeString, out _);
    }

    public bool IsValidDateTime(string dateTimeString)
    {
        return DateTime.TryParse(dateTimeString, out _);
    }

    public bool IsFutureDate(DateTime date)
    {
        return date > DateTime.Now;
    }

    public bool IsPastDate(DateTime date)
    {
        return date < DateTime.Now;
    }

    public bool IsToday(DateTime date)
    {
        return date.Date == DateTime.Today;
    }

    public bool IsWeekend(DateTime date)
    {
        return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
    }

    public bool IsWeekday(DateTime date)
    {
        return !IsWeekend(date);
    }
}

