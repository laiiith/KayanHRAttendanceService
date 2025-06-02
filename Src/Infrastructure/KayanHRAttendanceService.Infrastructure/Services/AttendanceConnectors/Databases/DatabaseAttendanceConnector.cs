using Dapper;
using KayanHRAttendanceService.Domain.Entities.Sqlite;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Data.Common;

namespace KayanHRAttendanceService.Infrastructure.Services.AttendanceConnectors.Databases;

public abstract class DatabaseAttendanceConnector<T>(ILogger<T> logger) : AttendanceConnector where T : class
{
    protected abstract Task<DbConnection> CreateDbConnection();

    protected void AddDatabaseParameters(SqlCommand command, List<SqlParameter> sqlParameters)
    {
        command.Parameters.Clear();
        foreach (SqlParameter sqlParameter in sqlParameters)
        {
            command.Parameters.Add(sqlParameter);
        }
    }

    protected void LogRecords(IEnumerable<AttendanceRecord> attendanceRecords)
    {
        foreach (var record in attendanceRecords)
        {
            Console.WriteLine($"Fetched Punch: {record.TId}");
            logger.LogInformation("Fetched attendance record with TID: {TId}", record.TId);
        }
    }

    public async Task UpdateFlagForFetchedDataAsync(List<AttendanceRecord> records, string updateProcedure)
    {
        if (records == null || records.Count == 0)
        {
            logger.LogWarning("No records provided for flag update.");
            return;
        }

        try
        {
            using var sqlConnection = await CreateDbConnection();
            using var sqlTransaction = await sqlConnection.BeginTransactionAsync();

            await sqlConnection.ExecuteAsync("DROP TEMPORARY TABLE IF EXISTS temp_tvp;", transaction: sqlTransaction);

            await sqlConnection.ExecuteAsync("CREATE TEMPORARY TABLE temp_tvp(tid INT,flag INT DEFAULT(1));", transaction: sqlTransaction);

            var createTempTableSql = GetCreateTempTableSql();

            await sqlConnection.ExecuteAsync(createTempTableSql, transaction: sqlTransaction);

            var insertParams = records.Select(r => new { tid = r.TId });

            var insertSql = GetInsertTempTableSql();

            await sqlConnection.ExecuteAsync(insertSql, insertParams, sqlTransaction);

            await sqlConnection.ExecuteAsync(updateProcedure, commandType: CommandType.StoredProcedure, transaction: transaction);

            await sqlTransaction.CommitAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error in UpdateFlagForFetchedDataAsync.");
            throw;
        }
    }

    private string GetCreateTempTableSql() => "CREATE TEMPORARY TABLE temp_tvp(tid INT,flag INT DEFAULT 1);";

    private string GetInsertTempTableSql() => "INSERT INTO temp_tvp (tid) VALUES (@tid);";
}