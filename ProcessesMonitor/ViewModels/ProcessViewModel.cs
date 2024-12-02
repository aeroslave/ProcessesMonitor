using ProcessesMonitor.Models;

namespace ProcessesMonitor.ViewModels;

public record ProcessViewModel(
    int Id,
    string ProcessName,
    long MemoryUsage,
    double CpuUsage)
{
    public static ProcessViewModel FromEntity(ProcessEntity entity) =>
        new(entity.Id, entity.ProcessName, entity.MemoryUsage, entity.CpuUsage);
};
