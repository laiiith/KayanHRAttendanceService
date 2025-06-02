using System.Data;
using System.Data.Common;
using Dapper;
using KayanHRAttendanceService.Domain.Entities.General;
using KayanHRAttendanceService.Domain.Entities.Sqlite;
using KayanHRAttendanceService.Domain.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KayanHRAttendanceService.Infrastructure.Services.AttendanceConnectors.Databases;

public class MSSqlServerConnector(IOptions<IntegrationSettings> settings, ILogger<MSSqlServerConnector> logger)
    : DatabaseAttendanceConnector, IAttendanceConnector
{
    public async Task<List<AttendanceRecord>> FetchAttendanceDataAsync()
    {
        try
        {
            using var sqlConnection = await CreateDbConnection() as SqlConnection;

            var tvp = new SqlParameter("@TempTVP", SqlDbType.Structured)
            {
                TypeName = "dbo.TempTVPType",
                Value = CreateDataTable([], 1)
            };

            var parameters = new DynamicParameters();
            parameters.Add("@TempTVP", tvp.Value, dbType: DbType.Object);

            var records = (await sqlConnection.QueryAsync<AttendanceRecord>(
                sql: settings.Value.GetDataProcedure,
                param: parameters,
                commandType: CommandType.StoredProcedure
            )).ToList();

            return records;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while fetching data from SQL Server using Dapper.");
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
            if (!int.TryParse(item.TId, out int tid)) continue;
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
