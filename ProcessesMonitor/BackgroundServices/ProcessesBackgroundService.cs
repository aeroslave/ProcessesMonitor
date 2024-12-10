using ProcessesMonitor.Services;

namespace ProcessesMonitor.BackgroundServices;

public class ProcessesBackgroundService(IProcessesService processesService, ILogger<ProcessesBackgroundService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await processesService.UpdateProcessesAsync();
            }
            catch (Exception exception)
            {
                logger.LogError(exception, $"Something went wrong: {exception.Message}");
            }
        }
    }
}
