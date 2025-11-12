namespace CommunityCar.Infrastructure.Services.Analytics;

/// <summary>
/// Constants used throughout the analytics reporting service.
/// </summary>
public static class AnalyticsConstants
{
    /// <summary>
    /// Default values for various calculations.
    /// </summary>
    public static class Defaults
    {
        /// <summary>
        /// Default bounce rate.
        /// </summary>
        public const double BounceRate = 0.35;

        /// <summary>
        /// Default engagement rate.
        /// </summary>
        public const double EngagementRate = 0.15;

        /// <summary>
        /// Default average session duration in minutes.
        /// </summary>
        public const double AverageSessionDurationMinutes = 12.5;

        /// <summary>
        /// Default average time spent on content in seconds.
        /// </summary>
        public const int AverageTimeSpentSeconds = 120;

        /// <summary>
        /// Default CPU usage percentage.
        /// </summary>
        public const double CpuUsage = 45.2;

        /// <summary>
        /// Default memory usage percentage.
        /// </summary>
        public const double MemoryUsage = 68.7;

        /// <summary>
        /// Default disk usage percentage.
        /// </summary>
        public const double DiskUsage = 52.1;

        /// <summary>
        /// Default cache hit rate percentage.
        /// </summary>
        public const double CacheHitRate = 89.5;

        /// <summary>
        /// Default uptime percentage.
        /// </summary>
        public const double UptimePercentage = 99.8;

        /// <summary>
        /// Default average response time in milliseconds.
        /// </summary>
        public const int AverageResponseTimeMs = 250;

        /// <summary>
        /// Default peak concurrent users.
        /// </summary>
        public const int PeakConcurrentUsers = 150;

        /// <summary>
        /// Default database connections count.
        /// </summary>
        public const int DatabaseConnections = 25;
    }

    /// <summary>
    /// User segment percentages.
    /// </summary>
    public static class UserSegments
    {
        /// <summary>
        /// Percentage of highly active users.
        /// </summary>
        public const double HighlyActivePercentage = 20.0;

        /// <summary>
        /// Percentage of moderately active users.
        /// </summary>
        public const double ModeratelyActivePercentage = 50.0;

        /// <summary>
        /// Percentage of low activity users.
        /// </summary>
        public const double LowActivityPercentage = 30.0;
    }

    /// <summary>
    /// Retention data day markers.
    /// </summary>
    public static class RetentionDays
    {
        /// <summary>
        /// Day 1 retention.
        /// </summary>
        public const int Day1 = 1;

        /// <summary>
        /// Day 7 retention.
        /// </summary>
        public const int Day7 = 7;

        /// <summary>
        /// Day 30 retention.
        /// </summary>
        public const int Day30 = 30;
    }

    /// <summary>
    /// Retention rates for different days.
    /// </summary>
    public static class RetentionRates
    {
        /// <summary>
        /// Day 1 retention rate.
        /// </summary>
        public const double Day1Rate = 85.5;

        /// <summary>
        /// Day 7 retention rate.
        /// </summary>
        public const double Day7Rate = 65.2;

        /// <summary>
        /// Day 30 retention rate.
        /// </summary>
        public const double Day30Rate = 45.8;
    }

    /// <summary>
    /// Trend change percentage.
    /// </summary>
    public const double TrendChangePercentage = 25.5;

    /// <summary>
    /// Version number for reports.
    /// </summary>
    public const string ReportVersion = "1.0";
}
