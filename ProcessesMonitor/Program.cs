using ProcessesMonitor.BackgroundServices;
using ProcessesMonitor.Services;

var builder = WebApplication.CreateBuilder(args);

const string policyName = "CorsPolicy";

builder.Services.AddCors(options =>
{
    options.AddPolicy(policyName,
        policyBuilder => policyBuilder
            .AllowAnyMethod()
            .AllowCredentials()
            .SetIsOriginAllowed((_) => true)
            .AllowAnyHeader());
});

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IProcessesService, ProcessesService>();
builder.Services.AddHostedService<ProcessesBackgroundService>();

builder.Services.AddSignalR();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(policyName);

app.MapHub<HighLoadWarningHub>("/highload");

app.MapControllers();

app.Run();
