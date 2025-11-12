namespace CommunityCar.Application.DTOs.Dashboard;

public class SystemHealthDto
{
    public string Status { get; set; } = "Healthy"; // Healthy, Degraded, Unhealthy
    public DateTime Timestamp { get; set; }
    public double ResponseTime { get; set; } // in milliseconds
    public long MemoryUsage { get; set; } // in bytes
    public double CpuUsage { get; set; } // percentage
    public long DiskUsage { get; set; } // in bytes
    public long TotalRequests { get; set; }
    public double ErrorRate { get; set; } // percentage
    public Dictionary<string, ServiceHealthDto> Services { get; set; } = new();
    public List<string> Issues { get; set; } = new();
}

public class ServiceHealthDto
{
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = "Healthy";
    public string Description { get; set; } = string.Empty;
    public DateTime LastChecked { get; set; }
    public TimeSpan? ResponseTime { get; set; }
}