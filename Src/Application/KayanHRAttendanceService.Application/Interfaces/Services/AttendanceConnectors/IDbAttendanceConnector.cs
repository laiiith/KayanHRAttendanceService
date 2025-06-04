using KayanHRAttendanceService.Domain.Entities.Sqlite;

namespace KayanHRAttendanceService.Application.Interfaces.Services.AttendanceConnectors;

public interface IDbAttendanceConnector : IAttendanceConnector
{
    Task UpdateFlagForFetchedDataAsync(List<AttendanceRecord> records);
}