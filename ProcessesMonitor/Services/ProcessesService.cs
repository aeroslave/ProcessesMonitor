using System.ComponentModel;
using System.Diagnostics;
using Microsoft.AspNetCore.SignalR;
using ProcessesMonitor.Models;

namespace ProcessesMonitor.Services;

public class ProcessesService(IHubContext<HighLoadWarningHub> hubContext, 
    ILogger<ProcessesService> logger) : IProcessesService
{
    private const string ReceiveMemoryHighLoad = "ReceiveMemoryHighLoad";
    private const string ReceiveCpuHighLoad = "ReceiveCpuHighLoad";
    private const long MbDivider = 1048576;

    private readonly Dictionary<int, ProcessEntity> _processDictionary = [];

    private readonly long _totalAvailableMemory = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / MbDivider;
    private readonly double _processorCount = Convert.ToDouble(Environment.ProcessorCount);
    private bool _isHighLoadCpu;
    private bool _isHighLoadMemory;

    public IReadOnlyCollection<ProcessEntity> Processes => _processDictionary.AsReadOnly().Values;

    public async Task UpdateProcessesAsync()
    {
        Process[] processes = Process.GetProcesses();

        foreach (var processViewModel in _processDictionary)
        {
            processViewModel.Value.IsActive = false;
        }

        await Task.Delay(1000);
        
        foreach (Process process in processes)
        {
            try
            {
                UpdateProcessesData(process);
            }
            catch (Win32Exception _)
            {
                // ignored
            }
            catch (Exception exception)
            {
                logger.LogError(exception.Message);
            }
        }

        await CheckHigLoadingAsync();

        IEnumerable<int> processesToDelete = _processDictionary
            .Where(it => !it.Value.IsActive)
            .Select(it => it.Key);

        foreach (var processId in processesToDelete)
        {
            _processDictionary.Remove(processId);
        }
    }

    private void UpdateProcessesData(Process process)
    {
        if (_processDictionary.TryGetValue(process.Id, out var processViewModel))
        {
            processViewModel.MemoryUsage = process.WorkingSet64 / MbDivider;
            var timeDelta = DateTime.Now.Subtract(processViewModel.LastTime).TotalMilliseconds;
            var totalMillisecondDelta = process.TotalProcessorTime.TotalMilliseconds - processViewModel.OldTotalProcessorTime;

            processViewModel.CpuUsage = Math.Round(totalMillisecondDelta / timeDelta / _processorCount * 100, 2);
            processViewModel.OldTotalProcessorTime = process.TotalProcessorTime.TotalMilliseconds;
            processViewModel.LastTime = DateTime.Now;
            processViewModel.IsActive = true;
        }
        else
        {
            _processDictionary.Add(process.Id, new ProcessEntity(process.Id, process.ProcessName)
            {
                MemoryUsage = process.WorkingSet64 / MbDivider,
                OldTotalProcessorTime = process.TotalProcessorTime.TotalMilliseconds,
                LastTime = DateTime.Now,
                IsActive = true
            });
        }
    }

    private async Task CheckHigLoadingAsync()
    {
        var totalCpuUsagePercentage = _processDictionary.Values.Sum(it => it.CpuUsage);
        var totalMemoryUsage = _processDictionary.Values.Sum(it => it.MemoryUsage);
        
        if (totalCpuUsagePercentage > 80)
        {
            if (!_isHighLoadCpu)
            {
                _isHighLoadCpu = true;
                await hubContext.Clients.All.SendAsync(ReceiveCpuHighLoad, _isHighLoadCpu);
                logger.LogWarning("Warning! CPU usage has exceeded 80%");
            }
        }
        else if (_isHighLoadCpu)
        {
            _isHighLoadCpu = false;
            await hubContext.Clients.All.SendAsync(ReceiveCpuHighLoad, _isHighLoadCpu);
            logger.LogInformation("CPU usage is less than 80%");
        }
        
        if (totalMemoryUsage/_totalAvailableMemory > 0.8)
        {
            if (!_isHighLoadMemory)
            {
                _isHighLoadMemory = true;
                await hubContext.Clients.All.SendAsync(ReceiveMemoryHighLoad, _isHighLoadMemory);
                logger.LogWarning("Warning! Available memory is running low!");
            }
        }
        else if (_isHighLoadMemory)
        {
            _isHighLoadMemory = false;
            await hubContext.Clients.All.SendAsync(ReceiveMemoryHighLoad, _isHighLoadMemory);
            logger.LogInformation("There is enough available memory!");
        }
    }
}
