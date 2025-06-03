using KayanHRAttendanceService.Application.Interfaces.Services;
using KayanHRAttendanceService.Domain.Entities.General;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KayanHRAttendanceService.WindowsService.Worker
{
    public class AttendanceWorker(IServiceProvider serviceProvider, IOptions<IntegrationSettings> settingsOptions, ILogger<AttendanceWorker> logger) : BackgroundService
    {
        private readonly IntegrationSettings _settings = settingsOptions.Value;


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("AttendanceWorker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = serviceProvider.CreateScope();
                    var syncService = scope.ServiceProvider.GetRequiredService<ISyncAttendanceData>();

                    await syncService.SyncData();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error in AttendanceWorker loop");
                }

                await Task.Delay(TimeSpan.FromSeconds(_settings.Interval), stoppingToken);
            }
        }
    }
}