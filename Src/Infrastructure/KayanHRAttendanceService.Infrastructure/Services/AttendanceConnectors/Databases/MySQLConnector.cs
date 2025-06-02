using Dapper;
using KayanHRAttendanceService.Domain.Entities.General;
using KayanHRAttendanceService.Domain.Entities.Sqlite;
using KayanHRAttendanceService.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using System.Data.Common;

namespace KayanHRAttendanceService.Infrastructure.Services.AttendanceConnectors.Databases;

public class MySQLConnector(IOptions<IntegrationSettings> settings, ILogger<MySQLConnector> logger) : DatabaseAttendanceConnector, IAttendanceConnector
{
    public async Task<List<AttendanceRecord>> FetchAttendanceDataAsync()
    {
        try
        {
            using var sqlConnection = await CreateDbConnection();
            var data = await sqlConnection.QueryAsync<AttendanceRecord>(settings.Value.GetDataProcedure, commandType: System.Data.CommandType.StoredProcedure);
            return [.. data];

        }
        catch (MySql.Data.MySqlClient.MySqlException ex)
        {
            switch (ex.Number)
            {
                case 0:
                    logger.LogError("Cannot connect to server.  Contact administrator");
                    break;
                case 1045:
                    logger.LogError("Invalid username/password, please try again");
                    break;
            }
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "MySqlException:");
            throw;
        }
    }

    protected override async Task<DbConnection> CreateDbConnection()
    {
        var sqlConnection = new MySqlConnection(settings.Value.ConnectionString);
        await sqlConnection.OpenAsync();
        return sqlConnection;
    }
}
