using KayanHRAttendanceService.Application.Interfaces.Data;
using KayanHRAttendanceService.Application.Interfaces.Services;
using KayanHRAttendanceService.Application.Interfaces.Services.AttendanceConnectors;
using KayanHRAttendanceService.Domain.Entities.General;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KayanHRAttendanceService.Infrastructure.Services;

public class DataPusherService(IAttendanceConnector attendanceConnector, IUnitOfWork unitOfWork, ILogger<DataPusherService> logger, IOptions<IntegrationSettings> settingsOptions, IKayanConnectorService kayanConnectorService) : IDataPusherService
{
    private readonly IntegrationSettings _settings = settingsOptions.Value;
    public async Task PushAsync()
    {
        logger.LogInformation("Starting to push attendance data to KayanHR...");

        var excludedStatuses = new[] { "2", "3", "401", "403", "500" };

        var records = await unitOfWork.AttendanceData.GetAllAsync(x => !excludedStatuses.Contains(x.Status), _settings.BatchSize);

        if (records == null || records.Count == 0)
        {
            logger.LogWarning("No attendance records to push.");
            return;
        }

        //var response =
        await kayanConnectorService.PushToKayanConnectorEndPoint(records);

        //MarkAsPushed();


        await unitOfWork.SaveChangesAsync();
        logger.LogInformation("Push operation completed.");
    }
}