using ProcessesMonitor.Models;

namespace ProcessesMonitor.Services;

public interface IProcessesService
{
    public IReadOnlyCollection<ProcessEntity> Processes { get; }
    public Task UpdateProcessesAsync();
}