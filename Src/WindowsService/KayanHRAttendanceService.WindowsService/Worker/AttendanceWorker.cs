using KayanHRAttendanceService.Application.Interfaces.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KayanHRAttendanceService.WindowsService.Worker
{
    public class AttendanceWorker(ISyncAttendanceData syncAttendanceData, ILogger<AttendanceWorker> logger) : BackgroundService
    {
        private readonly ISyncAttendanceData attendanceData;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("AttendanceWorker started.");


            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error in AttendanceWorker loop");
                }

                await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
            }
        }
    }
}
