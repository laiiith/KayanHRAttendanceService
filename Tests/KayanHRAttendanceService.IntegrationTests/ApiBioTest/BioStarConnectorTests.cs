using KayanHRAttendanceService.Application.DTO;
using KayanHRAttendanceService.Application.Implementation.Services.AttendanceConnectors.ApiBased;
using KayanHRAttendanceService.Application.Interfaces;
using KayanHRAttendanceService.Application.Interfaces.Data;
using KayanHRAttendanceService.Domain.Entities.General;
using KayanHRAttendanceService.Domain.Entities.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace KayanHRAttendanceService.IntegrationTests.ApiBioTest;

public class BioStarConnectorTests
{
    private readonly Mock<IHttpService> _mockHttpService = new();
    private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();
    private readonly Mock<IAttendanceDataRepository> _mockAttendanceRepo = new();
    private readonly Mock<ILogger<BioStarConnector>> _mockLogger = new();
    private readonly IOptions<IntegrationSettings> _mockOptions;

    public BioStarConnectorTests()
    {
        _mockOptions = Options.Create(new IntegrationSettings
        {
            Type = 4,
            BatchSize = 1,
            APIBulkEndpoint = "",
            ClientID = "",
            ClientSecret = "",
            DynamicDate = true,
            Interval = 1,
            FunctionMapping = new FunctionMapping
            {
                AttendanceIn = "attendance-in",
                AttendanceOut = "attendance-out",
                BreakIn = "break-in",
                BreakOut = "break-out",
                PermissionIn = "permission-in",
                PermissionOut = "permission-out",
                OvertimeIn = "overtime-in",
                OvertimeOut = "overtime-out"
            },
            Integration = new Integration
            {
                Server = "http://fake-server.com",
                Username = "user",
                Password = "pass",
                StartDate = "2025-01-01",
                EndDate = "2025-01-31",
                PageSize = "100"
            }
        });

        _mockUnitOfWork.Setup(u => u.AttendanceData).Returns(_mockAttendanceRepo.Object);
        _mockAttendanceRepo.Setup(r => r.GetLastPunchTime()).ReturnsAsync("2025-01-01T00:00:00Z");
    }

    [Fact]
    public async Task FetchAttendanceDataAsync_ReturnsAttendanceRecords()
    {
        var headers = new Dictionary<string, string> { { "bs-session-id", "test-session" } };
        var loginResponse = ApiResponse<object>.Success(null, headers);

        _mockHttpService.Setup(s => s.SendAsync<object>(It.Is<APIRequest>(r => r.Url.Contains("/api/login"))))
            .ReturnsAsync(loginResponse);

        var bioStarData = new List<BioStarEventDTO>
        {
            new BioStarEventDTO
            {
                Index = 1,
                Datetime = "2025-01-10T08:00:00Z",
                Tna_Key = "0",
                User_Id = new UserIdWrapper { User_Id = "EMP001" },
                Device_Id = new DeviceIdWrapper { Name = "Device1" }
            }
        };

        var eventsResponseData = new BioStarEventSearchResponseDTO
        {
            EventCollection = new EventCollection
            {
                Rows = bioStarData
            }
        };

        var eventsResponse = ApiResponse<BioStarEventSearchResponseDTO>.Success(eventsResponseData, new Dictionary<string, string>());

        _mockHttpService.Setup(s => s.SendAsync<BioStarEventSearchResponseDTO>(It.Is<APIRequest>(r => r.Url.Contains("/api/events/search"))))
            .ReturnsAsync(eventsResponse);

        var connector = new BioStarConnector(_mockHttpService.Object, _mockUnitOfWork.Object, _mockOptions, _mockLogger.Object);

        var result = await connector.FetchAttendanceDataAsync();

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("EMP001", result[0].EmployeeCode);
        Assert.Equal("attendance-in", result[0].Function);
    }

    [Fact]
    public async Task FetchAttendanceDataAsync_ReturnsEmpty_WhenLoginFails()
    {
        var failedLoginResponse = ApiResponse<object>.Fail("Invalid login");

        _mockHttpService.Setup(s => s.SendAsync<object>(It.Is<APIRequest>(r => r.Url.Contains("/api/login"))))
            .ReturnsAsync(failedLoginResponse);

        var connector = new BioStarConnector(_mockHttpService.Object, _mockUnitOfWork.Object, _mockOptions, _mockLogger.Object);

        var result = await connector.FetchAttendanceDataAsync();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task FetchAttendanceDataAsync_HandlesEmptyEventRows()
    {
        var headers = new Dictionary<string, string> { { "bs-session-id", "test-session" } };
        var loginResponse = ApiResponse<object>.Success(null, headers);

        _mockHttpService.Setup(s => s.SendAsync<object>(It.Is<APIRequest>(r => r.Url.Contains("/api/login"))))
            .ReturnsAsync(loginResponse);

        var emptyEventsData = new BioStarEventSearchResponseDTO
        {
            EventCollection = new EventCollection
            {
                Rows = new List<BioStarEventDTO>()
            }
        };

        var emptyEventsResponse = ApiResponse<BioStarEventSearchResponseDTO>.Success(emptyEventsData, new Dictionary<string, string>());

        _mockHttpService.Setup(s => s.SendAsync<BioStarEventSearchResponseDTO>(It.Is<APIRequest>(r => r.Url.Contains("/api/events/search"))))
            .ReturnsAsync(emptyEventsResponse);

        var connector = new BioStarConnector(_mockHttpService.Object, _mockUnitOfWork.Object, _mockOptions, _mockLogger.Object);

        var result = await connector.FetchAttendanceDataAsync();

        Assert.NotNull(result);
        Assert.Empty(result);
    }
}
