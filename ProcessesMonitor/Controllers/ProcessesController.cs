using Microsoft.AspNetCore.Mvc;
using ProcessesMonitor.Services;
using ProcessesMonitor.ViewModels;
using System.ComponentModel;

namespace ProcessesMonitor.Controllers;

[ApiController]
[Route("[controller]")]
public class ProcessesController : ControllerBase
{
    [Description("Method to get all processes")]
    [HttpGet(Name = "GetAllProcesses")]
    public async Task<IReadOnlyCollection<ProcessViewModel>> GetAllProcessesAsync()
    {
        IProcessesService processesService = HttpContext.RequestServices.GetRequiredService<IProcessesService>();
        
        return await Task.FromResult(processesService.Processes
            .Select(ProcessViewModel.FromEntity)
            .OrderByDescending(it => it.CpuUsage).ToList());
    }
}
