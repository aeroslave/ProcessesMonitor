using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;
using ProcessesMonitor.Models;

namespace ProcessesMonitor.Services;

public class ProcessesService : IProcessesService
{
    private readonly IHubContext<HighLoadWarningHub> _hubContext;
    private const long MbDivider = 1048576;
    private readonly Dictionary<int, ProcessEntity> _processViewModels;

    private const string ReceiveMemoryHighLoadMethodName = "ReceiveMemoryHighLoad";
    private const string ReceiveCpuHighLoad = "ReceiveCpuHighLoad";

    private readonly long _totalAvailableMemory;
    private readonly double _processorCount;
    private bool _isHighLoadCpu;
    private bool _isHighLoadMemory;

    public IReadOnlyCollection<ProcessEntity> Processes => _processViewModels.Values;

    public ProcessesService(IHubContext<HighLoadWarningHub> hubContext)
    {
        _hubContext = hubContext;

        _totalAvailableMemory = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / MbDivider;
        _processorCount = Convert.ToDouble(Environment.ProcessorCount);

        _processViewModels = new Dictionary<int, ProcessEntity>();
        Process[] processes = Process.GetProcesses();

        foreach (Process process in processes)
        {
            try
            {
                var processVm = new ProcessEntity(process.Id, process.ProcessName)
                {
                    MemoryUsage = process.WorkingSet64 / MbDivider,
                    OldTotalProcessorTime = process.TotalProcessorTime.TotalMilliseconds,
                    LastTime = DateTime.Now,
                    IsActive = true
                };

                _processViewModels.Add(process.Id, processVm);
            }
            catch (Exception _)
            {
                // ignored
            }
        }
    }

    public async Task UpdateProcessesAsync()
    {
        var processes = Process.GetProcesses().ToHashSet();
        
        foreach (KeyValuePair<int, ProcessEntity> processViewModel in _processViewModels)
        {
            processViewModel.Value.IsActive = false;
        }

        await Task.Delay(2000);
        
        foreach (Process process in processes)
        {
            try
            {
                UpdateProcessesData(process);
            }
            catch (Exception _)
            {
                // ignored
            }
        }

        await CheckHigLoadingAsync();

        IEnumerable<int> processesToDelete = _processViewModels
            .Where(it => !it.Value.IsActive)
            .Select(it => it.Key);

        foreach (var processId in processesToDelete)
        {
            _processViewModels.Remove(processId);
        }

        foreach (var process in processes)
        {
            process.Refresh();
        }
    }

    private void UpdateProcessesData(Process process)
    {
        if (_processViewModels.TryGetValue(process.Id, out var processViewModel))
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
            _processViewModels.Add(process.Id, new ProcessEntity(process.Id, process.ProcessName)
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
        var totalCpuUsagePercentage = _processViewModels.Values.Sum(it => it.CpuUsage);
        var totalMemoryUsage = _processViewModels.Values.Sum(it => it.MemoryUsage);
        
        if (totalCpuUsagePercentage > 80)
        {
            if (!_isHighLoadCpu)
            {
                _isHighLoadCpu = true;
                await _hubContext.Clients.All.SendAsync(ReceiveCpuHighLoad, _isHighLoadCpu);
            }
        }
        else if (_isHighLoadCpu)
        {
            _isHighLoadCpu = false;
            await _hubContext.Clients.All.SendAsync(ReceiveCpuHighLoad, _isHighLoadCpu);
        }
        
        if (totalMemoryUsage/_totalAvailableMemory > 0.8)
        {
            if (!_isHighLoadMemory)
            {
                _isHighLoadMemory = true;
                await _hubContext.Clients.All.SendAsync(ReceiveMemoryHighLoadMethodName, _isHighLoadMemory);
            }
        }
        else if (_isHighLoadMemory)
        {
            _isHighLoadMemory = false;
            await _hubContext.Clients.All.SendAsync(ReceiveMemoryHighLoadMethodName, _isHighLoadMemory);
        }
    }
}
