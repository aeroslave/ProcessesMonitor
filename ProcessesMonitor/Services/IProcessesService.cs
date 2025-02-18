using System.Collections.Concurrent;
using ProcessesMonitor.Models;

namespace ProcessesMonitor.Services;

public interface IProcessesService
{
    public ConcurrentDictionary<int, ProcessEntity> ProcessDictionary { get; }
    public Task UpdateProcessesAsync();
}
