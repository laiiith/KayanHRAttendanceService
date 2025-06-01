using KayanHRAttendanceService.Application.AttendanceConnector.Interfaces;
using KayanHRAttendanceService.Domain.Entities.Sqlite;

namespace KayanHRAttendanceService.Infrastructure.AttendanceConnector.Databases;

public class MSSqlServerConnector : AttendanceConnector, IAttendanceConnector
{
    public Task<List<AttendanceRecord>> FetchAttendanceDataAsync()
    {
        throw new NotImplementedException();
    }
}
