using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KayanHRAttendanceService.WindowsService.Worker
{
    public class AttendanceWorker : BackgroundService
    {
        private readonly ILogger<AttendanceWorker> _logger;

        public AttendanceWorker(ILogger<AttendanceWorker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AttendanceWorker started.");

            try
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize database");
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in AttendanceWorker loop");
                }

                await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
            }
        }
    }
}
