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
    class SQLiteTestConnector : MySQLConnector
    {
        private readonly DbConnection _connection;
        private readonly IOptions<IntegrationSettings> _settings;
        private readonly ILogger<MySQLConnector> _logger;

        public SQLiteTestConnector(IOptions<IntegrationSettings> settings, ILogger<MySQLConnector> logger, DbConnection connection)
            : base(settings, logger)
        {
            _connection = connection;
            _settings = settings;
            _logger = logger;
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
    public async Task FetchAttendanceDataAsync_ShouldReturnMappedRecords()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var createTableCmd = connection.CreateCommand();
        createTableCmd.CommandText = """
            CREATE TABLE AttendanceRecord (
                EmployeeCode TEXT,
                TId TEXT
            );
        """;
        await createTableCmd.ExecuteNonQueryAsync();

        var insertCmd = connection.CreateCommand();
        insertCmd.CommandText = """
            INSERT INTO AttendanceRecord (EmployeeCode, TId) VALUES ('E001', '1'), ('E002', '2');
        """;
        await insertCmd.ExecuteNonQueryAsync();

        var settings = Options.Create(new IntegrationSettings
        {
            Integration = new Integration
            {
                FetchDataProcedure = "SELECT * FROM AttendanceRecord",
                UpdateDataProcedure = "FAKE_PROC",
                ConnectionString = "Fake"
            }
        });

        var logger = new LoggerFactory().CreateLogger<MySQLConnector>();
        var connector = new SQLiteTestConnector(settings, logger, connection);

        var result = await connector.FetchAttendanceDataForTestAsync();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("E001", result[0].EmployeeCode);
        Assert.Equal("E002", result[1].EmployeeCode);
    }
}
