namespace FoodOrder.Application.DTOs.SystemHealth;

public class SystemHealthDto
{
    public string ApiStatus     { get; set; } = "Online";
    public string DbStatus      { get; set; } = "Unknown";
    public long   DbLatencyMs   { get; set; }
    public double MemoryUsedMb  { get; set; }
    public double GcHeapMb      { get; set; }
    public string Uptime        { get; set; } = "";
    public string MachineName   { get; set; } = "";
    public string OsDescription { get; set; } = "";
    public string DotNetVersion { get; set; } = "";
    public int    ProcessId     { get; set; }
    public int    ThreadCount   { get; set; }
    public List<HealthEventDto> RecentEvents { get; set; } = [];
}

public class HealthEventDto
{
    public DateTime Timestamp { get; set; }
    public string   Level     { get; set; } = "INFO";   // INFO | WARN | ERROR
    public string   Category  { get; set; } = "System";
    public string   Message   { get; set; } = "";
}
