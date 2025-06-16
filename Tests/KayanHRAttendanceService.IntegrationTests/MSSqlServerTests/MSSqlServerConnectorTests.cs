using KayanHRAttendanceService.Application.Implementation.Services.AttendanceConnectors.Databases;
using KayanHRAttendanceService.Domain.Entities.General;
using KayanHRAttendanceService.Domain.Entities.Sqlite;
using KayanHRAttendanceService.IntegrationTests.MSSqlServerTests.FakeDb;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace KayanHRAttendanceService.IntegrationTests.MSSqlServerTests;

public class MSSqlServerConnectorTests
{
    private readonly Mock<ILogger<MSSqlServerConnector>> mockLogger;
    private readonly IOptions<IntegrationSettings> settings;

    public MSSqlServerConnectorTests()
    {
        mockLogger = new Mock<ILogger<MSSqlServerConnector>>();
        settings = Options.Create(new IntegrationSettings
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
                ConnectionString = "FakeConnectionString",
                FetchDataProcedure = "GetAttendance",
                UpdateDataProcedure = "UpdateFlags"
            }
        });
    }

    private MSSqlServerConnector CreateConnector() =>
        new MSSqlServerConnector(settings, mockLogger.Object);

    [Fact]
    public async Task FetchAttendanceDataAsync_ShouldReturnMappedRecords()
    {
        var expected = new List<AttendanceRecord>
        {
            new AttendanceRecord
            {
                TId = "1",
                EmployeeCode = "EMP001",
                PunchTime = "2024-01-01T08:00:00Z",
                Function = "IN",
                MachineName = "DeviceA"
            }
        };

        var fakeConnection = new FakeDbConnection();
        var connector = new TestMSSqlServerConnector(settings, mockLogger.Object, fakeConnection);

        var result = await connector.FetchAttendanceDataAsync();

        Assert.NotNull(result);
        Assert.Single(result);
        var first = result.First();

        var normalizedFunction = first.Function switch
        {
            "attendance-in" => "IN",
            "attendance-out" => "OUT",
            _ => first.Function
        };

        Assert.Equal("1", first.TId);
        Assert.Equal("EMP001", first.EmployeeCode);
        Assert.Equal("2024-01-01T08:00:00Z", first.PunchTime);
        Assert.Equal("IN", normalizedFunction);
        Assert.Equal("DeviceA", first.MachineName);
    }

    [Fact]
    public async Task UpdateFlagForFetchedDataAsync_ShouldExecuteAllStatements()
    {
        var fakeConnection = new FakeDbConnection();
        var connector = new TestMSSqlServerConnector(settings, mockLogger.Object, fakeConnection);

        var records = new List<AttendanceRecord>
        {
            new AttendanceRecord { TId = "1", PunchTime = DateTime.Now.ToString() },
            new AttendanceRecord { TId = "2", PunchTime = DateTime.Now.ToString() }
        };

        await connector.UpdateFlagForFetchedDataAsync(records);

        Assert.True(fakeConnection.ExecutedCommands.Count >= 3);
    }

    [Fact]
    public async Task UpdateFlagForFetchedDataAsync_ShouldLogWarning_WhenNoRecords()
    {
        var connector = CreateConnector();

        await connector.UpdateFlagForFetchedDataAsync(new List<AttendanceRecord>());

        mockLogger.Verify(l => l.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No records provided for flag update.")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()
        ), Times.Once);
    }
}