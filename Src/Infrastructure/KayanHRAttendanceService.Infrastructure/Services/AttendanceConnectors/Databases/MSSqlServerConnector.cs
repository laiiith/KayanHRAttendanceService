using KayanHRAttendanceService.Domain.Entities.General;
using KayanHRAttendanceService.Domain.Entities.Sqlite;
using KayanHRAttendanceService.Domain.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data;
using System.Data.Common;

namespace KayanHRAttendanceService.Infrastructure.Services.AttendanceConnectors.Databases;

public class MSSqlServerConnector(IOptions<IntegrationSettings> settings, ILogger<MSSqlServerConnector> logger) : DatabaseAttendanceConnector, IAttendanceConnector
{
    public async Task<List<AttendanceRecord>> FetchAttendanceDataAsync()
    {
        try
        {
            using var sqlConnection = await CreateDbConnection() as SqlConnection;

            await using var sqlCommand = new SqlCommand(settings.Value.GetDataProcedure, sqlConnection)
            {
                CommandType = CommandType.StoredProcedure
            };
            AddDatabaseParameters(sqlCommand, [new("@TempTVP", SqlDbType.Structured, 1)]);

            var result = await sqlCommand.ExecuteNonQueryAsync();

            return new List<AttendanceRecord>();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error Occured Microsoft Sql Server");
            throw;
        }
    }
    private DataTable CreateDataTable(List<AttendanceRecord> data, int status)
    {
        var table = new DataTable();
        table.Columns.Add("tid", typeof(int));
        table.Columns.Add("flag", typeof(int));

        foreach (var item in data)
        {
            if (!int.TryParse(item.TId, out int tid))
            {
                continue;
            }

            table.Rows.Add(tid, status);
        }

        return table;
    }
    protected override async Task<DbConnection> CreateDbConnection()
    {
        var connection = new SqlConnection(settings.Value.ConnectionString);
        await connection.OpenAsync();
        return connection;
    }
}