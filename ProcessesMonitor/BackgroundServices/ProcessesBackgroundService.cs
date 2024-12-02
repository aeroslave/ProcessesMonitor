using ProcessesMonitor.Services;

namespace ProcessesMonitor.BackgroundServices;

public class ProcessesBackgroundService(IProcessesService processesService) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await processesService.UpdateProcessesAsync();
        }
    }
}