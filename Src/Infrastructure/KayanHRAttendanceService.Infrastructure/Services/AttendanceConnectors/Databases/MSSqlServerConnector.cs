using KayanHRAttendanceService.Domain.Entities.Sqlite;
using KayanHRAttendanceService.Domain.Interfaces;
using System.Data.Common;

namespace KayanHRAttendanceService.Infrastructure.Services.AttendanceConnectors.Databases;

public class MSSqlServerConnector : DatabaseAttendanceConnector, IAttendanceConnector
{
    public Task<List<AttendanceRecord>> FetchAttendanceDataAsync()
    {
        throw new NotImplementedException();
    }

    protected override Task<DbConnection> CreateDbConnection()
    {
        throw new NotImplementedException();
    }
}
