using KayanHRAttendanceService.Application.Implementation.Services.AttendanceConnectors.Databases;
using KayanHRAttendanceService.Domain.Entities.General;
using KayanHRAttendanceService.Domain.Entities.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Data.Common;

namespace KayanHRAttendanceService.IntegrationTests.PostgreSqlConnectorTests;

public class PostgreSqlConnectorTests
{
    private readonly Mock<IOptions<IntegrationSettings>> _mockOptions;
    private readonly Mock<ILogger<PostgreSqlConnector>> _mockLogger;
    private readonly TestPostgreSqlConnector _connector;

    public PostgreSqlConnectorTests()
    {
        _mockOptions = new Mock<IOptions<IntegrationSettings>>();
        _mockLogger = new Mock<ILogger<PostgreSqlConnector>>();

        var integrationSettings = new IntegrationSettings
        {
            Type = 3,
            APIBulkEndpoint = "",
            BatchSize = 1,
            ClientID = "",
            ClientSecret = "",
            DynamicDate = true,
            Interval = 1,
            FunctionMapping = new FunctionMapping
            {
                AttendanceIn = "0",
                AttendanceOut = "1",
                BreakIn = "2",
                BreakOut = "3",
                PermissionIn = "4",
                PermissionOut = "5",
                OvertimeIn = "6",
                OvertimeOut = "7"
            },
            Integration = new Integration
            {
                ConnectionString = "Host=localhost;Username=test;Password=test;Database=testdb",
                FetchDataProcedure = "GetAttendanceData",
                UpdateDataProcedure = "UpdateAttendanceData"
            }
        };

        _mockOptions.Setup(x => x.Value).Returns(integrationSettings);

        _connector = new TestPostgreSqlConnector(_mockOptions.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task FetchAttendanceDataAsync_ReturnsData()
    {
        var expectedData = new List<AttendanceRecord>
        {
            new AttendanceRecord { TId = "1",PunchTime=DateTime.Now.ToString() },
            new AttendanceRecord { TId = "2",PunchTime=DateTime.Now.ToString() }
        };

        _connector.SetQueryResult(expectedData);

        var result = await _connector.FetchAttendanceDataAsync();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("1", result[0].TId);
        Assert.Equal("2", result[1].TId);
    }

    private class TestPostgreSqlConnector : PostgreSqlConnector
    {
        private IEnumerable<AttendanceRecord> _mockQueryResult = new List<AttendanceRecord>();

        public TestPostgreSqlConnector(IOptions<IntegrationSettings> settings, ILogger<PostgreSqlConnector> logger)
            : base(settings, logger)
        {
        }

        public void SetQueryResult(IEnumerable<AttendanceRecord> data)
        {
            _mockQueryResult = data;
        }

        protected override Task<DbConnection> CreateDbConnection()
        {
            return Task.FromResult<DbConnection>(result: new KayanHRAttendanceService.IntegrationTests.PostgreSqlConnectorTests.FakeDbToPostgreSql.FakeDbConnection(_mockQueryResult));
        }
    }
}