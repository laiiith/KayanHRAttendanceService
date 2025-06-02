using KayanHRAttendanceService.Domain.Entities.Sqlite;

namespace KayanHRAttendanceService.Domain.Interfaces;

public interface IAttendanceConnector
{
    Task<List<AttendanceRecord>> FetchAttendanceDataAsync();
}
