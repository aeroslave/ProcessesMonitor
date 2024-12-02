# ProcessesMonitor
## Common description:
The system is based on a continuously updated background service. To read data, a controller with a single method GetAllProcesses is implemented - a Get method that returns a list of view models of the type: 
```json
{
    "id": 0,
    "processName": "string",
    "memoryUsage": 0,
    "cpuUsage": 0
}
```
id – process identifier, processName – process name (not obvious, isn’t?), memoryUsage – allocated memory in megabytes, cpuUsage – percentage of processor time used by this process.

I decided to use SignalR to notify about high CPU/memory load.
The project has a minimalist architecture, but has the possibility of expansion. When the project grows, you should to create the Application layer, to which you can move the Services and ViewModels folders. And also you should to create the Domain layer, to which you can move the Models folder. Optionally, Infrastructure layer, to which you can move BackgroundServices. Implement dependencies between projects according to DDD – ProcessesMonitor->Infrastructure->Application->Domain.

## Clients:
To get the list use REST client. To get data on high load it is recommended to use libraries from Microsoft (@microsoft/signalr для js). For example you can use [this](https://github.com/aeroslave/ProcessesMonitorClient)

## Launch:
To test the entire system, you need to run the .Net project. Copy the address and paste it into the client file in the url field (line 34). In theory, this should be put into the configuration file, but at this stage I think this solution is sufficient. Then you can run the client, click the Get Processes button to get a list of processes.
If the load on the processor/memory increases, a corresponding warning will appear under the button.

