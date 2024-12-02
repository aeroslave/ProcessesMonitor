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
    public IReadOnlyCollection<ProcessViewModel> GetAllProcesses()
    {
        IProcessesService processesService = HttpContext.RequestServices.GetRequiredService<IProcessesService>();

        return processesService.Processes
            .Select(ProcessViewModel.FromEntity)
            .OrderByDescending(it => it.CpuUsage).ToList();
    }
}
