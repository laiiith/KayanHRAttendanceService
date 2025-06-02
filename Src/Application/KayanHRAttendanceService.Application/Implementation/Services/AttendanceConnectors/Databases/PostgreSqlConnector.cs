using Dapper;
using KayanHRAttendanceService.Application.Interfaces.Services.AttendanceConnectors;
using KayanHRAttendanceService.Domain.Entities.General;
using KayanHRAttendanceService.Domain.Entities.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Data.Common;

namespace KayanHRAttendanceService.Application.Implementation.Services.AttendanceConnectors.Databases;

public class PostgreSqlConnector(IOptions<IntegrationSettings> settings, ILogger<PostgreSqlConnector> logger) : DatabaseAttendanceConnector<PostgreSqlConnector>(logger), IAttendanceConnector
{
    public async Task<List<AttendanceRecord>> FetchAttendanceDataAsync()
    {
        using var sqlConnection = await CreateDbConnection();

        if (sqlConnection is not NpgsqlConnection npgSqlConnection)
            throw new InvalidOperationException("NpgsqlConnection Expected");

        var data = await npgSqlConnection.QueryAsync<AttendanceRecord>(settings.Value.GetDataProcedure, commandType: System.Data.CommandType.StoredProcedure);
        LogRecords(data);
        return data.AsList();
    }

    protected override async Task<DbConnection> CreateDbConnection()
    {
        var NpgsqlConnection = new NpgsqlConnection(settings.Value.ConnectionString);
        await NpgsqlConnection.OpenAsync();
        return NpgsqlConnection;
    }

    protected override string GetDropTempTableSql() => "DROP TABLE IF EXISTS temp_tvp";

    protected override string GetCreateTempTableSql() => "CREATE TEMP TABLE temp_tvp (tid INT, flag INT DEFAULT 1) ON COMMIT DROP";
}