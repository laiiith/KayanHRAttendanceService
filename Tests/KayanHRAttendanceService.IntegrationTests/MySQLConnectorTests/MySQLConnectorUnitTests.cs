using System.Data.Common;
using KayanHRAttendanceService.Application.Implementation.Services.AttendanceConnectors.Databases;
using KayanHRAttendanceService.Domain.Entities.General;
using KayanHRAttendanceService.Domain.Entities.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace KayanHRAttendanceService.IntegrationTests.MySQLConnectorTests;

public class MySQLConnectorTests
{
    private readonly Mock<IOptions<IntegrationSettings>> _mockOptions;
    private readonly Mock<ILogger<MySQLConnector>> _mockLogger;
    private readonly TestMySQLConnector _connector;

    public MySQLConnectorTests()
    {
        _mockOptions = new Mock<IOptions<IntegrationSettings>>();
        _mockLogger = new Mock<ILogger<MySQLConnector>>();

        var integrationSettings = new IntegrationSettings
        {
            Type = 1,
            APIBulkEndpoint = "",
            BatchSize = 1,
            ClientID = "",
            ClientSecret = "",
            DynamicDate = true,
            Interval = 1,
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
                ConnectionString = "FakeConnectionString",
                FetchDataProcedure = "SELECT * FROM KayanAttendance",
                UpdateDataProcedure = ""
            }
        };

        _mockOptions.Setup(x => x.Value).Returns(integrationSettings);

        _connector = new TestMySQLConnector(_mockOptions.Object, _mockLogger.Object);
    }
    [Fact]
    public async Task FetchAttendanceDataAsync_ShouldHandleNullValuesGracefully()
    {
        var records = new List<AttendanceRecord>
    {
        new AttendanceRecord
        {
            TId = null, EmployeeCode = null, Function = null, PunchTime = null, MachineName = null
        }
    };

        _connector.SetMockQueryResult(records);
        var result = await _connector.FetchAttendanceDataAsync();

        Assert.Single(result);
        Assert.Null(result[0].TId);
        Assert.Null(result[0].EmployeeCode);
    }

    [Fact]
    public async Task FetchAttendanceDataAsync_ReturnsMockedData()
    {
        var expectedData = new List<AttendanceRecord>
        {
            new AttendanceRecord { TId = "1", EmployeeCode = "EMP001", Function = "CheckIn", PunchTime = "2025-06-04 08:01:00" },
            new AttendanceRecord { TId = "2", EmployeeCode = "EMP001", Function = "CheckOut", PunchTime = "2025-06-04 17:00:00" }
        };

        _connector.SetMockQueryResult(expectedData);

        var result = await _connector.FetchAttendanceDataAsync();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("EMP001", result[0].EmployeeCode);
        Assert.Equal("attendance-in", result[0].Function);
        Assert.Equal("EMP001", result[1].EmployeeCode);
        Assert.Equal("attendance-out", result[1].Function);
    }

    [Fact]
    public async Task FetchAttendanceDataAsync_ShouldThrowException_WhenSettingsAreInvalid()
    {
        var settings = new IntegrationSettings
        {
            Integration = new Integration
            {
                ConnectionString = "",
                FetchDataProcedure = ""
            }
        };

        var mockOptions = new Mock<IOptions<IntegrationSettings>>();
        mockOptions.Setup(x => x.Value).Returns(settings);

        var logger = new Mock<ILogger<MySQLConnector>>();
        var connector = new MySQLConnector(mockOptions.Object, logger.Object);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => connector.FetchAttendanceDataAsync());
        Assert.Equal("FetchDataProcedure or ConnectionString is Empty", exception.Message);
    }


    private class TestMySQLConnector : MySQLConnector
    {
        private IEnumerable<AttendanceRecord> _mockQueryResult = new List<AttendanceRecord>();

        public TestMySQLConnector(IOptions<IntegrationSettings> settings, ILogger<MySQLConnector> logger)
            : base(settings, logger)
        {
        }

        public void SetMockQueryResult(IEnumerable<AttendanceRecord> data)
        {
            _mockQueryResult = data;
        }

        protected override Task<DbConnection> CreateDbConnection()
        {
            return Task.FromResult<DbConnection>(new KayanHRAttendanceService.IntegrationTests.MySQLConnectorTests.FakeDbToMySql.FakeDbConnection(_mockQueryResult));
        }
    }
}
