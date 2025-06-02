using KayanHRAttendanceService.Domain.Entities.General;
using KayanHRAttendanceService.Domain.Entities.Sqlite;
using KayanHRAttendanceService.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Data.Common;

namespace KayanHRAttendanceService.Infrastructure.Services.AttendanceConnectors.Databases;

public class PostgreSqlConnector(IOptions<IntegrationSettings> settings, ILogger<PostgreSqlConnector> logger) : DatabaseAttendanceConnector, IAttendanceConnector
{
    public async Task<List<AttendanceRecord>> FetchAttendanceDataAsync()
    {
        try
        {
            using var sqlConnection = await CreateDbConnection() as NpgsqlConnection;
            using var sqlCommand = new NpgsqlCommand(settings.Value.GetDataProcedure, sqlConnection);
            sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
            using var reader = await sqlCommand.ExecuteReaderAsync();
            return MapToList(reader);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in PostgreSqlConnector");
            throw;
        }
    }

    protected override async Task<DbConnection> CreateDbConnection()
    {
        var NpgsqlConnection = new NpgsqlConnection(settings.Value.ConnectionString);
        await NpgsqlConnection.OpenAsync();
        return NpgsqlConnection;
    }
}