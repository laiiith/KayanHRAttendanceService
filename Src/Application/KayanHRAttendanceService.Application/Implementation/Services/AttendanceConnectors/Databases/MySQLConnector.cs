using Dapper;
using KayanHRAttendanceService.Application.Interfaces.Services.AttendanceConnectors;
using KayanHRAttendanceService.Domain.Entities.General;
using KayanHRAttendanceService.Domain.Entities.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using System.Data.Common;

namespace KayanHRAttendanceService.Application.Implementation.Services.AttendanceConnectors.Databases;

public class MySQLConnector(IOptions<IntegrationSettings> settings, ILogger<MySQLConnector> logger) : DatabaseAttendanceConnector<MySQLConnector>(logger), IAttendanceConnector
{
    public async Task<List<AttendanceRecord>> FetchAttendanceDataAsync()
    {
        using var sqlConnection = await CreateDbConnection();
        var data = await sqlConnection.QueryAsync<AttendanceRecord>(settings.Value.GetDataProcedure, commandType: System.Data.CommandType.StoredProcedure);
        LogRecords(data);
        return data.AsList();
    }

    protected override async Task<DbConnection> CreateDbConnection()
    {
        var sqlConnection = new MySqlConnection(settings.Value.ConnectionString);
        await sqlConnection.OpenAsync();
        return sqlConnection;
    }
}