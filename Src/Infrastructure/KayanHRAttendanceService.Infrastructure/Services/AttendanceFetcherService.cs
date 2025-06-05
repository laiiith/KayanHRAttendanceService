using KayanHRAttendanceService.Application.Interfaces.Data;
using KayanHRAttendanceService.Application.Interfaces.Services;
using KayanHRAttendanceService.Application.Interfaces.Services.AttendanceConnectors;
using KayanHRAttendanceService.Domain.Entities.Sqlite;
using Microsoft.Extensions.Logging;

namespace KayanHRAttendanceService.Infrastructure.Services;

public class AttendanceFetcherService(IAttendanceConnector attendanceConnector, IUnitOfWork unitOfWork, ILogger<AttendanceFetcherService> logger) : IAttendanceFetcherService
{
    public async Task FetchAsync()
    {
        logger.LogInformation("Starting attendance data fetch...");

        var records = await attendanceConnector.FetchAttendanceDataAsync();

        if (records is null || records.Count == 0)
        {
            logger.LogWarning("No attendance records were fetched.");
            return;
        }

        logger.LogInformation("Fetched {Count} attendance records. Saving to database...", records.Count);

        var existingRecords = await unitOfWork.AttendanceData.GetAllAsync(r =>
            records.Select(p => p.EmployeeCode).Contains(r.EmployeeCode) &&
            records.Select(p => p.PunchTime).Contains(r.PunchTime));

        var existingPairs = new HashSet<(string EmployeeCode, string PunchTime)>(
                        existingRecords.Select(r => (r.EmployeeCode!, r.PunchTime)));

        var newRecords = records.Where(p => !existingPairs.Contains((p.EmployeeCode!, p.PunchTime))).ToList();

        await unitOfWork.AttendanceData.AddAsync([.. newRecords]);

        await unitOfWork.SaveChangesAsync();

        logger.LogInformation("Attendance records successfully saved.");

        await UpdateExternalSourceFlagsIfApplicable(records);
    }

    private async Task UpdateExternalSourceFlagsIfApplicable(List<AttendanceRecord> records)
    {
        if (attendanceConnector is IDbAttendanceConnector dbConnector)
        {
            logger.LogInformation("Updating fetched flag on external data source...");
            await dbConnector.UpdateFlagForFetchedDataAsync(records);
        }
    }
}