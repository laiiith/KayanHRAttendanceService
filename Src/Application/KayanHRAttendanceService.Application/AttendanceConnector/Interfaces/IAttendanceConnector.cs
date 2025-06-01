using KayanHRAttendanceService.Domain.Entities.Sqlite;

namespace KayanHRAttendanceService.Application.AttendanceConnector.Interfaces;

public interface IAttendanceConnector
{
    Task<List<AttendanceRecord>> FetchAttendanceDataAsync();
}
