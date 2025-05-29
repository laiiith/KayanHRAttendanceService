using KayanHRAttendanceService.WindowsService.Storage.Models;

namespace KayanHRAttendanceService.WindowsService.AttendanceConnector.Databases;

public class PostgreSqlConnector : AttendanceConnector, IAttendanceConnector
{
    public Task<List<AttendanceRecord>> FetchAttendanceAsync()
    {
        throw new NotImplementedException();
    }
}
