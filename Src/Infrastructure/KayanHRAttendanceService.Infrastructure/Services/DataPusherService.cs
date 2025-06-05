using KayanHRAttendanceService.Application.Interfaces.Data;
using KayanHRAttendanceService.Application.Interfaces.Services;
using KayanHRAttendanceService.Application.Interfaces.Services.AttendanceConnectors;
using KayanHRAttendanceService.Domain.Entities.General;
using KayanHRAttendanceService.Domain.Entities.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KayanHRAttendanceService.Infrastructure.Services;

public class DataPusherService(IAttendanceConnector attendanceConnector, IUnitOfWork unitOfWork, ILogger<DataPusherService> logger, IOptions<IntegrationSettings> settingsOptions, IKayanConnectorService kayanConnectorService) : IDataPusherService
{
    private readonly IAttendanceConnector _attendanceConnector = attendanceConnector;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<DataPusherService> _logger = logger;
    private readonly IKayanConnectorService _kayanConnectorService = kayanConnectorService;
    private readonly IntegrationSettings _settings = settingsOptions.Value;
    private static readonly string[] _excludedStatuses = { "2", "3", "401", "403", "500" };


    public async Task PushAsync()
    {
        _logger.LogInformation("Starting attendance data push to KayanHR...");

        try
        {
            var pendingRecords = await _unitOfWork.AttendanceData
                .GetAllAsync(x => !_excludedStatuses.Contains(x.Status), _settings.BatchSize);

            if (pendingRecords == null || pendingRecords.Count == 0)
            {
                _logger.LogInformation("No attendance records found for pushing.");
                return;
            }

            _logger.LogInformation("Fetched {Count} attendance records for pushing.", pendingRecords.Count);

            var (isSuccess, responseStatusId) = await _kayanConnectorService
                .PushToKayanConnectorEndPoint(pendingRecords);


            var statusId = responseStatusId != 200
                          ? 0
                          : isSuccess ? 2 : 3;

            if (_attendanceConnector is IDbAttendanceConnector dbConnector)
            {
                await dbConnector.UpdateFlagForFetchedDataAsync(pendingRecords, responseStatusId);
            }

            if (isSuccess)
            {
                await MarkAsPushedAsync(pendingRecords, statusId);
            }

            _logger.LogInformation("Attendance data push operation completed.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while pushing attendance data.");
            throw;
        }
    }

    private async Task MarkAsPushedAsync(List<AttendanceRecord> punches, int statusId)
    {
        if (punches is null || punches.Count == 0)
            return;

        foreach (var record in punches)
        {
            record.Status = statusId.ToString();
        }

        await _unitOfWork.AttendanceData.UpdateAsync(punches);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Marked {Count} attendance records as pushed with status {Status}.", punches.Count, statusId);
    }
}