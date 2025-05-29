using KayanHRAttendanceService.WindowsService.Storage.Models;

namespace KayanHRAttendanceService.WindowsService.AttendanceConnector.Databases;

public class MSSqlServerConnector : AttendanceConnector, IAttendanceConnector
{
    public Task<List<AttendanceRecord>> FetchAttendanceAsync()
    {
        throw new NotImplementedException();
    }
}
