using Dapper;
using KayanHRAttendanceService.Domain.Entities.General;
using KayanHRAttendanceService.Domain.Entities.Sqlite;
using KayanHRAttendanceService.Domain.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data;
using System.Data.Common;

namespace KayanHRAttendanceService.Infrastructure.Services.AttendanceConnectors.Databases;

public class MSSqlServerConnector(IOptions<IntegrationSettings> settings, ILogger<MSSqlServerConnector> logger) : DatabaseAttendanceConnector<MSSqlServerConnector>(logger), IAttendanceConnector
{
    public async Task<List<AttendanceRecord>> FetchAttendanceDataAsync()
    {
        using var sqlConnection = await CreateDbConnection() as SqlConnection;

        if (sqlConnection is not SqlConnection sqlServerConnection)
            throw new InvalidOperationException("SqlConnection Expected");

        var data = await sqlServerConnection.QueryAsync<AttendanceRecord>(settings.Value.GetDataProcedure, commandType: CommandType.StoredProcedure);
        LogRecords(data);
        return [.. data];
    }

    protected override async Task<DbConnection> CreateDbConnection()
    {
        var connection = new SqlConnection(settings.Value.ConnectionString);
        await connection.OpenAsync();
        return connection;
    }

    protected override string GetDropTempTableSql() => "IF OBJECT_ID('tempdb..#TempTVP') IS NOT NULL DROP TABLE #TempTVP;";

    protected override string GetCreateTempTableSql() => "CREATE TABLE #TempTVP(tid INT,flag INT DEFAULT 1);";

    protected override string GetInsertTempTableSql() => "INSERT INTO #TempTVP (tid) VALUES (@tid)";

}
