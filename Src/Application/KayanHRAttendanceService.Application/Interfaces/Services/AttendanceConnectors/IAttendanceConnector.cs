using KayanHRAttendanceService.Domain.Entities.Sqlite;

namespace KayanHRAttendanceService.Application.Interfaces.Services.AttendanceConnectors;

public interface IAttendanceConnector
{
    Task<List<AttendanceRecord>> FetchAttendanceDataAsync();
}