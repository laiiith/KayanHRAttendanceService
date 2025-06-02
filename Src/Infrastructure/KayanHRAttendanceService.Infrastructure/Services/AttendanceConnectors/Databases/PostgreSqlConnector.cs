using KayanHRAttendanceService.Domain.Entities.Sqlite;
using KayanHRAttendanceService.Domain.Interfaces;

namespace KayanHRAttendanceService.Infrastructure.Services.AttendanceConnectors.Databases;

public class PostgreSqlConnector : DatabaseAttendanceConnector, IAttendanceConnector
{
    public Task<List<AttendanceRecord>> FetchAttendanceDataAsync()
    {
        throw new NotImplementedException();
    }
}