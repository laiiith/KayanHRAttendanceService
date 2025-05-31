using KayanHRAttendanceService.WindowsService.Services.IServices;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KayanHRAttendanceService.WindowsService.Worker
{
    public class AttendanceWorker(ILogger<AttendanceWorker> logger) : BackgroundService
    {
        private readonly IAttendanceFetcherService _fetcherService;
        private readonly IDataPusherService _pusherService;

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
