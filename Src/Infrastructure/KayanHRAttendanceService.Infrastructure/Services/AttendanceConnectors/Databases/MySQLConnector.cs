using Dapper;
using KayanHRAttendanceService.Domain.Entities.General;
using KayanHRAttendanceService.Domain.Entities.Sqlite;
using KayanHRAttendanceService.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using System.Data.Common;

namespace KayanHRAttendanceService.Infrastructure.Services.AttendanceConnectors.Databases;

public class MySQLConnector(IOptions<IntegrationSettings> settings, ILogger<MySQLConnector> logger) : DatabaseAttendanceConnector<MySQLConnector>(logger), IAttendanceConnector
{
    private readonly ILogger<MySQLConnector> _logger = logger;

    public async Task<List<AttendanceRecord>> FetchAttendanceDataAsync()
    {
        try
        {
            using var sqlConnection = await CreateDbConnection();
            var data = await sqlConnection.QueryAsync<AttendanceRecord>(settings.Value.GetDataProcedure, commandType: System.Data.CommandType.StoredProcedure);
            LogRecords(data);
            return data.AsList();
        }
        catch (MySqlException ex) when (ex.Number == 0)
        {
            _logger.LogError("Cannot connect to server. Contact administrator.");
            throw;
        }
        catch (MySqlException ex) when (ex.Number == 1045)
        {
            _logger.LogError("Invalid username/password, please try again.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while fetching attendance data.");
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
