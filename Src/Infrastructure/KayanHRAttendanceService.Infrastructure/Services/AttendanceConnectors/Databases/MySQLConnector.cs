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

            using var sqlCommand = sqlConnection.CreateCommand();
            sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
            sqlCommand.CommandText = settings.Value.GetDataProcedure;

            using var sqlDataReader = await sqlCommand.ExecuteReaderAsync();

            return MapToList(sqlDataReader);

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
                default:
                    logger.LogError(ex, "MySqlException:");
                    break;
            }
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
