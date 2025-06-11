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
            throw new InvalidOperationException("FetchDataProcedure or ConnectionString is Empty");
        }

        using var connection = await CreateDbConnection();

        var records = await connection.QueryAsync<AttendanceRecord>(GetFetchQuery);

        LogRecords(records);

        return NormalizeFunctionValues(records);
    }

    protected override async Task<DbConnection> CreateDbConnection()
    {
        var NpgsqlConnection = new NpgsqlConnection(_settings.Integration.ConnectionString);
        await NpgsqlConnection.OpenAsync();
        return NpgsqlConnection;
    }

    protected override string GetDropTempTableSql()
        => "DROP TABLE IF EXISTS Temp";

    protected override string GetCreateTempTableSql()
        => "CREATE TEMP TABLE Temp (TId VARCHAR, Flag INT DEFAULT 1) ON COMMIT DROP";

    private string GetFetchQuery
        => $"SELECT * FROM {_settings.Integration.FetchDataProcedure}()";
}