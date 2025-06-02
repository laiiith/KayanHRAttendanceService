using Microsoft.Data.SqlClient;
using System.Data.Common;

namespace KayanHRAttendanceService.Infrastructure.Services.AttendanceConnectors.Databases;

public abstract class DatabaseAttendanceConnector : AttendanceConnector
{
    protected abstract Task<DbConnection> CreateDbConnection();
    protected void AddDatabaseParameters(SqlCommand command, List<SqlParameter> sqlParameters)
    {
        command.Parameters.Clear();
        foreach (SqlParameter sqlParameter in sqlParameters)
        {
            command.Parameters.Add(sqlParameter);
        }
    }
}
