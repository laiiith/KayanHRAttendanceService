﻿using Dapper;
using KayanHRAttendanceService.Application.Interfaces.Services.AttendanceConnectors;
using KayanHRAttendanceService.Domain.Entities.General;
using KayanHRAttendanceService.Domain.Entities.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using System.Data.Common;

namespace KayanHRAttendanceService.Application.Implementation.Services.AttendanceConnectors.Databases;

public class MySQLConnector(IOptions<IntegrationSettings> settingsOptions, ILogger<MySQLConnector> logger) : DatabaseAttendanceConnector<MySQLConnector>(settingsOptions, logger), IDbAttendanceConnector
{
    private readonly IntegrationSettings _settings = settingsOptions.Value;

    public async Task<List<AttendanceRecord>> FetchAttendanceDataAsync()
    {
        if (string.IsNullOrEmpty(_settings.Integration.FetchDataProcedure) || string.IsNullOrEmpty(_settings.Integration.ConnectionString))
        {
            throw new InvalidOperationException("FetchDataProcedure or ConnectionString is Empty");
        }

        using var sqlConnection = await CreateDbConnection();

        var data = await sqlConnection.QueryAsync<AttendanceRecord>(_settings.Integration.FetchDataProcedure, commandType: System.Data.CommandType.StoredProcedure);

        LogRecords(data);

        return NormalizeFunctionValues(data);
    }

    protected override async Task<DbConnection> CreateDbConnection()
    {
        var sqlConnection = new MySqlConnection(_settings.Integration.ConnectionString);
        await sqlConnection.OpenAsync();
        return sqlConnection;
    }

    protected override string GetCreateTempTableSql()
        => "CREATE TEMPORARY TABLE Temp(TId VARCHAR(150),flag INT DEFAULT (1))";

    protected override string GetInsertTempTableSql()
        => "INSERT INTO Temp(TId,Flag) VALUES (@TId,@flag)";

    protected override string GetDropTempTableSql()
        => "DROP TEMPORARY TABLE IF EXISTS Temp";
}