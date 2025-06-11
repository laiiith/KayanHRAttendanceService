using Dapper;
using KayanHRAttendanceService.Application.Interfaces.Services.AttendanceConnectors;
using KayanHRAttendanceService.Domain.Entities.General;
using KayanHRAttendanceService.Domain.Entities.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Data.Common;

namespace KayanHRAttendanceService.Application.Implementation.Services.AttendanceConnectors.Databases;

public class PostgreSqlConnector(IOptions<IntegrationSettings> settingsOptions, ILogger<PostgreSqlConnector> logger) : DatabaseAttendanceConnector<PostgreSqlConnector>(settingsOptions, logger), IDbAttendanceConnector
{
    private readonly IntegrationSettings _settings = settingsOptions.Value;

    public async Task<List<AttendanceRecord>> FetchAttendanceDataAsync()
    {
        if (string.IsNullOrEmpty(_settings.Integration.FetchDataProcedure) || string.IsNullOrEmpty(_settings.Integration.ConnectionString))
        {
            logger.LogWarning("FetchDataProcedure or ConnectionString is Empty");
            return [];
        }
        using var sqlConnection = await CreateDbConnection();
        var data = await sqlConnection.QueryAsync<AttendanceRecord>(_settings.Integration.FetchDataProcedure, commandType: System.Data.CommandType.StoredProcedure);
        LogRecords(data);
        return NormalizeFunctionValues(data);
    }

    protected override async Task<DbConnection> CreateDbConnection()
    {
        var NpgsqlConnection = new NpgsqlConnection(_settings.Integration.ConnectionString);
        await NpgsqlConnection.OpenAsync();
        return NpgsqlConnection;
    }

    protected override string GetDropTempTableSql() => "DROP TABLE IF EXISTS temp_tvp";

    protected override string GetCreateTempTableSql() => "CREATE TEMP TABLE temp_tvp (tid INT, flag INT DEFAULT 1) ON COMMIT DROP";
}