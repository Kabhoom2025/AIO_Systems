namespace Pharmacy.API.DTOs;

public class PharmacyHealthDto
{
    public string ApiStatus { get; set; } = "Online";
    public string DbStatus { get; set; } = string.Empty;
    public long DbLatencyMs { get; set; }
    public double MemoryUsedMb { get; set; }
    public string Uptime { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
    public string DotNetVersion { get; set; } = string.Empty;
}
