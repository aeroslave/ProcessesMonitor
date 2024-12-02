namespace ProcessesMonitor.Models;

public record ProcessEntity(int Id, 
    string ProcessName)
{
    public double OldTotalProcessorTime { get; set; }
    public long MemoryUsage { get; set; }
    public double CpuUsage { get; set; }
    public DateTime LastTime { get; set; }
    public bool IsActive { get; set; }
}
