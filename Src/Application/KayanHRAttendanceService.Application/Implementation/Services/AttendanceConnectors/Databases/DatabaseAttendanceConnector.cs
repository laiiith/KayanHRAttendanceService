using Dapper;
using KayanHRAttendanceService.Domain.Entities.General;
using KayanHRAttendanceService.Domain.Entities.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data;
using System.Data.Common;

namespace KayanHRAttendanceService.Application.Implementation.Services.AttendanceConnectors.Databases;

public abstract class DatabaseAttendanceConnector<T>(IOptions<IntegrationSettings> settingsOptions, ILogger<T> logger) where T : class
{
    private readonly IntegrationSettings _settings = settingsOptions.Value;

    protected abstract Task<DbConnection> CreateDbConnection();

    protected void LogRecords(IEnumerable<AttendanceRecord> attendanceRecords)
    {
        foreach (var record in attendanceRecords)
        {
            Console.WriteLine($"Fetched Punch: {record.TId}");
            logger.LogInformation("Fetched attendance record with TID: {TId}", record.TId);
        }
    }

    public async Task UpdateFlagForFetchedDataAsync(List<AttendanceRecord> records, int statusID = 1)
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

            var dropTempTableSql = GetDropTempTableSql();

            await sqlConnection.ExecuteAsync(dropTempTableSql, transaction: sqlTransaction);

            var createTempTableSql = GetCreateTempTableSql();

            await sqlConnection.ExecuteAsync(createTempTableSql, transaction: sqlTransaction);

            var insertParams = records.Select(r => new { tid = r.TId, flag = statusID });

            var insertSql = GetInsertTempTableSql();

            await sqlConnection.ExecuteAsync(insertSql, insertParams, sqlTransaction);

            await sqlConnection.ExecuteAsync(_settings.Integration.UpdateDataProcedure, commandType: CommandType.StoredProcedure, transaction: sqlTransaction);

            await sqlTransaction.CommitAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error in UpdateFlagForFetchedDataAsync.");
            throw;
        }
    }

    protected virtual string GetCreateTempTableSql() => "CREATE TEMPORARY TABLE temp_tvp(tid INT,flag INT DEFAULT (1))";

    protected virtual string GetInsertTempTableSql() => "INSERT INTO temp_tvp (tid,flag) VALUES (@tid,@flag)";

    protected virtual string GetDropTempTableSql() => "DROP TEMPORARY TABLE IF EXISTS temp_tvp";
}