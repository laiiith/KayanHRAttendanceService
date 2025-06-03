using Dapper;
using KayanHRAttendanceService.Application.Interfaces.Services.AttendanceConnectors;
using KayanHRAttendanceService.Domain.Entities.General;
using KayanHRAttendanceService.Domain.Entities.Sqlite;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data;
using System.Data.Common;

namespace KayanHRAttendanceService.Application.Implementation.Services.AttendanceConnectors.Databases;

public class MSSqlServerConnector(IOptions<IntegrationSettings> settingsOptions, ILogger<MSSqlServerConnector> logger) : DatabaseAttendanceConnector<MSSqlServerConnector>(settingsOptions, logger), IDbAttendanceConnector
{
    private readonly IntegrationSettings _settings = settingsOptions.Value;

    public async Task<List<AttendanceRecord>> FetchAttendanceDataAsync()
    {
        if (string.IsNullOrEmpty(_settings.Integration.FetchDataProcedure) || string.IsNullOrEmpty(_settings.Integration.ConnectionString))
        {
            logger.LogWarning("FetchDataProcedure or ConnectionString is Empty");
            return [];
        }
        using var dbConnection = await CreateDbConnection();

        var data = await dbConnection.QueryAsync<AttendanceRecord>(_settings.Integration.FetchDataProcedure, commandType: CommandType.StoredProcedure);

        LogRecords(data);
        return data.AsList();
    }

    protected override async Task<DbConnection> CreateDbConnection()
    {
        var connection = new SqlConnection(_settings.Integration.ConnectionString);
        await connection.OpenAsync();
        return connection;
    }

    protected override string GetDropTempTableSql() => "IF OBJECT_ID('tempdb..#TempTVP') IS NOT NULL DROP TABLE #TempTVP;";

    protected override string GetCreateTempTableSql() => "CREATE TABLE #TempTVP(tid INT,flag INT DEFAULT 1);";

    protected override string GetInsertTempTableSql() => "INSERT INTO #TempTVP (tid) VALUES (@tid)";
}