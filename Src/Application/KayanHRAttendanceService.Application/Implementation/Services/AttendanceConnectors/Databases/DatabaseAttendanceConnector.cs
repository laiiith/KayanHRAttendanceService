﻿using Dapper;
using KayanHRAttendanceService.Domain.Entities.General;
using KayanHRAttendanceService.Domain.Entities.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data;
using System.Data.Common;

namespace KayanHRAttendanceService.Application.Implementation.Services.AttendanceConnectors.Databases;

public abstract class DatabaseAttendanceConnector<T>(IOptions<IntegrationSettings> settingsOptions, ILogger<T> logger) : AttendanceConnectors(settingsOptions) where T : class
{
    private readonly IntegrationSettings _settings = settingsOptions.Value;

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

            var insertParams = records.Select(r => new { r.TId, Flag = statusID });

            var insertSql = GetInsertTempTableSql();

            await sqlConnection.ExecuteAsync(insertSql, insertParams, sqlTransaction);

            await sqlConnection.ExecuteAsync(_settings.Integration.UpdateDataProcedure ?? string.Empty, commandType: CommandType.StoredProcedure, transaction: sqlTransaction);

            await sqlTransaction.CommitAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error in UpdateFlagForFetchedDataAsync.");
            throw;
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

    protected List<AttendanceRecord> NormalizeFunctionValues(IEnumerable<AttendanceRecord> records)
    => [.. records.Select(x => new AttendanceRecord
    {
        TId = x.TId,
        EmployeeCode = x.EmployeeCode,
        Function = MapFunction(x.Function),
        ID = x.ID,
        MachineName = x.MachineName,
        MachineSerialNo = x.MachineSerialNo,
        Status = x.Status,
        PunchTime = x.PunchTime,
    })];

    protected abstract Task<DbConnection> CreateDbConnection();

    protected abstract string GetCreateTempTableSql();

    protected abstract string GetInsertTempTableSql();

    protected abstract string GetDropTempTableSql();
}