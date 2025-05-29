using KayanHRAttendanceService.WindowsService.Storage.Models;

namespace KayanHRAttendanceService.WindowsService.AttendanceConnector.Databases;

public class MySQLConnector : AttendanceConnector, IAttendanceConnector
{
    public Task<List<AttendanceRecord>> FetchAttendanceAsync()
    {
        throw new NotImplementedException();
    }
}
