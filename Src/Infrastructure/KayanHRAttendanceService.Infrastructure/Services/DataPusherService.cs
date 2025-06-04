using KayanHRAttendanceService.Application.DTO;
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
        logger.LogInformation("Starting attendance data push to KayanHR...");

        try
        {
            var excludedStatuses = new[] { "2", "3", "401", "403", "500" };

            var pendingRecords = await unitOfWork.AttendanceData
                .GetAllAsync(x => !excludedStatuses.Contains(x.Status), _settings.BatchSize);

            if (pendingRecords == null || pendingRecords.Count == 0)
            {
                logger.LogInformation("No attendance records found for pushing.");
                return;
            }

            logger.LogInformation("Fetched {Count} attendance records for pushing.", pendingRecords.Count);

            var (response, initialStatusId) = await kayanConnectorService
                .PushToKayanConnectorEndPoint(pendingRecords);

            int statusId = response?.Response.IsSuccess == true ? 2 : 3;

            if (attendanceConnector is IDbAttendanceConnector dbConnector)
            {
                await dbConnector.UpdateFlagForFetchedDataAsync(pendingRecords, statusId);
            }

            if (response?.Data?.ListPunches is { Count: > 0 })
            {
                await MarkAsPushedAsync(response.Data.ListPunches, statusId);
            }

            logger.LogInformation("Attendance data push operation completed.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while pushing attendance data.");
            throw;
        }
    }

    private async Task MarkAsPushedAsync(List<KayanConnectorAttendanceDTO> listPunches, int statusId)
    {
        if (listPunches == null || listPunches.Count == 0)
            return;

        var tids = listPunches.Select(p => p.tid).Distinct().ToList();

        var recordsToUpdate = await unitOfWork.AttendanceData
            .GetAllAsync(x => tids.Contains(x.TId));

        if (recordsToUpdate == null || recordsToUpdate.Count == 0)
            return;

        foreach (var record in recordsToUpdate)
        {
            record.Status = statusId.ToString();
        }

        await unitOfWork.AttendanceData.UpdateAsync(recordsToUpdate);
        await unitOfWork.SaveChangesAsync();

        logger.LogInformation("Marked {Count} attendance records as pushed with status {Status}.", recordsToUpdate.Count, statusId);
    }
}