using KayanHRAttendanceService.Application.Interfaces.Services;
using KayanHRAttendanceService.Domain.Entities.General;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KayanHRAttendanceService.WindowsService.Worker
{
    public class AttendanceWorker(ISyncAttendanceData syncAttendanceData, IOptions<IntegrationSettings> settingsOptions, ILogger<AttendanceWorker> logger) : BackgroundService
    {
        private readonly ISyncAttendanceData _attendanceData = syncAttendanceData;
        private readonly IntegrationSettings _settings = settingsOptions.Value;


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("AttendanceWorker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _attendanceData.SyncData();
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