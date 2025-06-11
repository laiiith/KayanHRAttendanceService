using Dapper;
using KayanHRAttendanceService.Application.Implementation.Services.AttendanceConnectors.Databases;
using KayanHRAttendanceService.Domain.Entities.General;
using KayanHRAttendanceService.Domain.Entities.Sqlite;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data.Common;

namespace KayanHRAttendanceService.IntegrationTests.MySQLConnectorTests;

public class MySQLConnectorUnitTests
{
    private class SQLiteTestConnector : MySQLConnector
    {
        private readonly DbConnection _connection;
        private readonly IOptions<IntegrationSettings> _settings;

        public SQLiteTestConnector(IOptions<IntegrationSettings> settings, ILogger<MySQLConnector> logger, DbConnection connection)
            : base(settings, logger)
        {
            _connection = connection;
            _settings = settings;
        }

        protected override Task<DbConnection> CreateDbConnection()
        {
            return Task.FromResult(_connection);
        }

        public async Task<List<AttendanceRecord>> FetchAttendanceDataForTestAsync()
        {
            using var sqlConnection = await CreateDbConnection();
            var data = await sqlConnection.QueryAsync<AttendanceRecord>(
                _settings.Value.Integration.FetchDataProcedure,
                commandType: System.Data.CommandType.Text);

            return data.AsList();
        }
    }

    [Fact]
    public async Task FetchAttendanceDataAsync_ShouldReturnMappedRecords_MockMySQLStructure()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var createTableCmd = connection.CreateCommand();
        createTableCmd.CommandText = """
            CREATE TABLE KayanAttendance (
                TID INTEGER PRIMARY KEY,
                EmployeeCardNumber TEXT,
                AttendanceDate TEXT,
                FunctionType TEXT,
                MachineName TEXT,
                Flag INTEGER DEFAULT 0
            );
        """;
        await createTableCmd.ExecuteNonQueryAsync();

        var insertCmd = connection.CreateCommand();
        insertCmd.CommandText = """
            INSERT INTO KayanAttendance (TID, EmployeeCardNumber, AttendanceDate, FunctionType, MachineName)
            VALUES
            (1, 'EMP001', '2025-06-04 08:01:00', 'CheckIn', 'Machine_A'),
            (2, 'EMP001', '2025-06-04 17:00:00', 'CheckOut', 'Machine_A'),
            (3, 'EMP002', '2025-06-04 08:05:00', 'CheckIn', 'Machine_B');
        """;
        await insertCmd.ExecuteNonQueryAsync();

        var settings = Options.Create(new IntegrationSettings
        {
            Type = 1,
            Interval = 5,
            APIBulkEndpoint = "https://fake.api/endpoint",
            ClientID = "fake-client-id",
            ClientSecret = "fake-client-secret",
            BatchSize = 100,
            DynamicDate = false,
            FunctionMapping = new FunctionMapping
            {
                AttendanceIn = "CheckIn",
                AttendanceOut = "CheckOut",
                BreakIn = "BreakIn",
                BreakOut = "BreakOut",
                PermissionIn = "PermissionIn",
                PermissionOut = "PermissionOut",
                OvertimeIn = "OTIn",
                OvertimeOut = "OTOut"
            },
            Integration = new Integration
            {
                FetchDataProcedure = """
            SELECT 
                TID AS TId,
                EmployeeCardNumber AS EmployeeCode,
                AttendanceDate AS PunchTime,
                FunctionType AS `Function`,
                MachineName
            FROM KayanAttendance
        """,
                UpdateDataProcedure = "NOT_USED_IN_TEST",
                ConnectionString = "Fake"
            }
        });


        var logger = new LoggerFactory().CreateLogger<MySQLConnector>();
        var connector = new SQLiteTestConnector(settings, logger, connection);

        var result = await connector.FetchAttendanceDataForTestAsync();

        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Equal("EMP001", result[0].EmployeeCode);
        Assert.Equal("CheckIn", result[0].Function);
    }
}
