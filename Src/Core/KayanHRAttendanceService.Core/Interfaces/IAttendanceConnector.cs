using KayanHRAttendanceService.Entities.Sqlite;

namespace KayanHRAttendanceService.Core.Interfaces;

public interface IAttendanceConnector
{
    Task<List<AttendanceRecord>> FetchAttendanceAsync();
}
