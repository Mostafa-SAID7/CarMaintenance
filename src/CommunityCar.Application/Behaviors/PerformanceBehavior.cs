using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace CommunityCar.Application.Behaviors;

public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
    private readonly IPerformanceMonitor _performanceMonitor;
    private readonly ICurrentUserService _currentUserService;
    private readonly Meter _meter;
    private readonly Counter<long> _requestCounter;
    private readonly Histogram<double> _requestDuration;

    public PerformanceBehavior(
        ILogger<PerformanceBehavior<TRequest, TResponse>> logger,
        IPerformanceMonitor performanceMonitor,
        ICurrentUserService currentUserService)
    {
        _logger = logger;
        _performanceMonitor = performanceMonitor;
        _currentUserService = currentUserService;

        _meter = new Meter("CommunityCar.Application");
        _requestCounter = _meter.CreateCounter<long>("mediatr_requests_total", "Total number of MediatR requests");
        _requestDuration = _meter.CreateHistogram<double>("mediatr_request_duration", "ms", "Duration of MediatR requests");
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = _currentUserService.GetCurrentUserId();

        using var activity = Activity.Current?.Source.StartActivity($"MediatR {requestName}", ActivityKind.Internal);
        activity?.SetTag("request.type", requestName);
        activity?.SetTag("user.id", userId ?? "anonymous");

        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Record request start
            _performanceMonitor.RecordRequestStart(requestName, userId);

            var response = await next();

            stopwatch.Stop();

            // Record metrics
            _requestCounter.Add(1, new KeyValuePair<string, object?>("request_type", requestName));
            _requestDuration.Record(stopwatch.Elapsed.TotalMilliseconds,
                new KeyValuePair<string, object?>("request_type", requestName));

            // Log performance
            _logger.LogInformation("Request {RequestName} completed in {ElapsedMilliseconds}ms for user {UserId}",
                requestName, stopwatch.ElapsedMilliseconds, userId ?? "anonymous");

            // Check for slow requests
            if (stopwatch.ElapsedMilliseconds > GetSlowRequestThreshold(request))
            {
                _logger.LogWarning("Slow request detected: {RequestName} took {ElapsedMilliseconds}ms",
                    requestName, stopwatch.ElapsedMilliseconds);

                _performanceMonitor.RecordSlowRequest(requestName, stopwatch.Elapsed, userId);
            }

            // Record successful completion
            _performanceMonitor.RecordRequestEnd(requestName, stopwatch.Elapsed, true, userId);

            activity?.SetStatus(ActivityStatusCode.Ok);
            activity?.SetTag("performance.duration_ms", stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            // Record failed request
            _performanceMonitor.RecordRequestEnd(requestName, stopwatch.Elapsed, false, userId);

            // Log performance even for failed requests
            _logger.LogError(ex, "Request {RequestName} failed after {ElapsedMilliseconds}ms for user {UserId}",
                requestName, stopwatch.ElapsedMilliseconds, userId ?? "anonymous");

            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.SetTag("error.type", ex.GetType().Name);
            activity?.SetTag("performance.duration_ms", stopwatch.ElapsedMilliseconds);

            throw;
        }
    }

    private int GetSlowRequestThreshold(TRequest request)
    {
        // Different thresholds for different types of requests
        if (request is IQuery)
            return 1000; // 1 second for queries
        if (request is ICommand)
            return 5000; // 5 seconds for commands
        return 2000; // 2 seconds default
    }
}

public interface IPerformanceMonitor
{
    void RecordRequestStart(string requestType, string? userId);
    void RecordRequestEnd(string requestType, TimeSpan duration, bool success, string? userId);
    void RecordSlowRequest(string requestType, TimeSpan duration, string? userId);
    PerformanceMetrics GetMetrics();
    IEnumerable<SlowRequestInfo> GetSlowRequests(int count = 50);
}

public class PerformanceMetrics
{
    public int TotalRequests { get; set; }
    public int SuccessfulRequests { get; set; }
    public int FailedRequests { get; set; }
    public double AverageResponseTime { get; set; }
    public double MedianResponseTime { get; set; }
    public double Percentile95ResponseTime { get; set; }
    public double Percentile99ResponseTime { get; set; }
    public Dictionary<string, RequestTypeMetrics> MetricsByType { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}

public class RequestTypeMetrics
{
    public string RequestType { get; set; } = string.Empty;
    public int TotalRequests { get; set; }
    public int SuccessfulRequests { get; set; }
    public int FailedRequests { get; set; }
    public double AverageResponseTime { get; set; }
    public double MinResponseTime { get; set; }
    public double MaxResponseTime { get; set; }
    public int SlowRequestsCount { get; set; }
}

public class SlowRequestInfo
{
    public string RequestType { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public string? UserId { get; set; }
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object>? AdditionalData { get; set; }
}

public class InMemoryPerformanceMonitor : IPerformanceMonitor
{
    private readonly ConcurrentDictionary<string, RequestTypeMetrics> _metrics = new();
    private readonly ConcurrentQueue<SlowRequestInfo> _slowRequests = new();
    private readonly ILogger<InMemoryPerformanceMonitor> _logger;
    private int _totalRequests;
    private int _successfulRequests;
    private int _failedRequests;
    private readonly ConcurrentBag<double> _responseTimes = new();

    public InMemoryPerformanceMonitor(ILogger<InMemoryPerformanceMonitor> logger)
    {
        _logger = logger;
    }

    public void RecordRequestStart(string requestType, string? userId)
    {
        Interlocked.Increment(ref _totalRequests);
    }

    public void RecordRequestEnd(string requestType, TimeSpan duration, bool success, string? userId)
    {
        if (success)
            Interlocked.Increment(ref _successfulRequests);
        else
            Interlocked.Increment(ref _failedRequests);

        _responseTimes.Add(duration.TotalMilliseconds);

        // Update type-specific metrics
        var typeMetrics = _metrics.GetOrAdd(requestType, _ => new RequestTypeMetrics { RequestType = requestType });

        Interlocked.Increment(ref typeMetrics.TotalRequests);
        if (success)
            Interlocked.Increment(ref typeMetrics.SuccessfulRequests);
        else
            Interlocked.Increment(ref typeMetrics.FailedRequests);

        // Update response time stats (simplified - in production use proper statistics)
        typeMetrics.AverageResponseTime = (typeMetrics.AverageResponseTime * (typeMetrics.TotalRequests - 1) + duration.TotalMilliseconds) / typeMetrics.TotalRequests;
        typeMetrics.MinResponseTime = Math.Min(typeMetrics.MinResponseTime == 0 ? duration.TotalMilliseconds : typeMetrics.MinResponseTime, duration.TotalMilliseconds);
        typeMetrics.MaxResponseTime = Math.Max(typeMetrics.MaxResponseTime, duration.TotalMilliseconds);
    }

    public void RecordSlowRequest(string requestType, TimeSpan duration, string? userId)
    {
        var slowRequest = new SlowRequestInfo
        {
            RequestType = requestType,
            Duration = duration,
            UserId = userId,
            Timestamp = DateTime.UtcNow
        };

        _slowRequests.Enqueue(slowRequest);

        // Keep only the last 1000 slow requests
        while (_slowRequests.Count > 1000 && _slowRequests.TryDequeue(out _)) { }

        var typeMetrics = _metrics.GetOrAdd(requestType, _ => new RequestTypeMetrics { RequestType = requestType });
        Interlocked.Increment(ref typeMetrics.SlowRequestsCount);
    }

    public PerformanceMetrics GetMetrics()
    {
        var responseTimes = _responseTimes.ToArray();
        Array.Sort(responseTimes);

        return new PerformanceMetrics
        {
            TotalRequests = _totalRequests,
            SuccessfulRequests = _successfulRequests,
            FailedRequests = _failedRequests,
            AverageResponseTime = responseTimes.Length > 0 ? responseTimes.Average() : 0,
            MedianResponseTime = responseTimes.Length > 0 ? responseTimes[responseTimes.Length / 2] : 0,
            Percentile95ResponseTime = GetPercentile(responseTimes, 0.95),
            Percentile99ResponseTime = GetPercentile(responseTimes, 0.99),
            MetricsByType = new Dictionary<string, RequestTypeMetrics>(_metrics),
            LastUpdated = DateTime.UtcNow
        };
    }

    public IEnumerable<SlowRequestInfo> GetSlowRequests(int count = 50)
    {
        return _slowRequests.OrderByDescending(x => x.Duration).Take(count);
    }

    private double GetPercentile(double[] sortedArray, double percentile)
    {
        if (sortedArray.Length == 0) return 0;

        var index = (int)Math.Ceiling(sortedArray.Length * percentile) - 1;
        index = Math.Max(0, Math.Min(index, sortedArray.Length - 1));
        return sortedArray[index];
    }
}

public class PerformanceMonitoringBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IPerformanceMonitor _performanceMonitor;
    private readonly ILogger<PerformanceMonitoringBehavior<TRequest, TResponse>> _logger;

    public PerformanceMonitoringBehavior(
        IPerformanceMonitor performanceMonitor,
        ILogger<PerformanceMonitoringBehavior<TRequest, TResponse>> logger)
    {
        _performanceMonitor = performanceMonitor;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestType = typeof(TRequest).Name;
        var startTime = DateTime.UtcNow;

        try
        {
            var response = await next();
            var duration = DateTime.UtcNow - startTime;

            // Additional performance checks
            if (request is IPerformanceCritical)
            {
                if (duration > TimeSpan.FromSeconds(5))
                {
                    _logger.LogWarning("Performance critical request {RequestType} exceeded threshold: {Duration}ms",
                        requestType, duration.TotalMilliseconds);
                }
            }

            return response;
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            _logger.LogError(ex, "Request {RequestType} failed after {Duration}ms", requestType, duration.TotalMilliseconds);
            throw;
        }
    }
}

public interface IPerformanceCritical
{
    // Marker interface for performance-critical requests
}

public interface IQuery : IRequest
{
    // Marker interface for query requests
}

public interface ICommand : IRequest
{
    // Marker interface for command requests
}

public static class PerformanceExtensions
{
    public static IServiceCollection AddPerformanceMonitoring(this IServiceCollection services)
    {
        services.AddSingleton<IPerformanceMonitor, InMemoryPerformanceMonitor>();
        services.AddMediatR(cfg =>
        {
            cfg.AddBehavior(typeof(PerformanceBehavior<,>));
            cfg.AddBehavior(typeof(PerformanceMonitoringBehavior<,>));
        });

        return services;
    }

    public static IApplicationBuilder UsePerformanceMonitoring(this IApplicationBuilder app)
    {
        // Add performance monitoring middleware if needed
        return app;
    }
}
