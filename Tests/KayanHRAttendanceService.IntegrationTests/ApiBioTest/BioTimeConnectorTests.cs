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

public class BioTimeConnectorTests
{
    private readonly Mock<IHttpService> _mockHttpService = new();
    private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();
    private readonly Mock<IAttendanceDataRepository> _mockAttendanceRepo = new();
    private readonly Mock<ILogger<BioTimeConnector>> _mockLogger = new();
    private readonly IOptions<IntegrationSettings> _mockOptions;

    public BioTimeConnectorTests()
    {
        _mockOptions = Options.Create(new IntegrationSettings
        {
            Type = 3,
            APIBulkEndpoint = "",
            BatchSize = 1,
            ClientID = "",
            ClientSecret = "",
            DynamicDate = true,
            Interval = 1,
            Integration = new Integration
            {
                Server = "http://fake-server.com",
                Username = "user",
                Password = "pass",
                StartDate = "2025-01-01",
                EndDate = "2025-01-31",
                PageSize = "100",
            },
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
            }
        });

        _mockUnitOfWork.Setup(u => u.AttendanceData)
                       .Returns(_mockAttendanceRepo.Object);

        _mockAttendanceRepo.Setup(r => r.GetLastPunchTime())
                           .ReturnsAsync("2025-01-01 00:00:00");
    }

    [Fact]
    public async Task FetchAttendanceDataAsync_ReturnsAttendanceRecords()
    {
        var tokenResponse = new ApiResponse<TokenDTO>
        {
            IsSuccess = true,
            Data = new TokenDTO { AccessToken = "fake-token" }
        };

        var attendanceData = new List<BioTimePunches>
        {
            new BioTimePunches
            {
                ID = 1,
                EmployeeCode = "EMP001",
                PunchTime = "2025-01-15 08:00:00",
                PunchStatus = "0",
                MachineName = "MachineA",
                MachineSerialNo = "123456"
            }
        };

        var attendanceResponse = new ApiResponse<BioTimeResponseDTO>
        {
            IsSuccess = true,
            Data = new BioTimeResponseDTO
            {
                BioTimePunches = attendanceData,
                NextUrl = null
            }
        };

        _mockHttpService.Setup(s => s.SendAsync<TokenDTO>(It.IsAny<APIRequest>()))
                        .ReturnsAsync(tokenResponse);

        _mockHttpService.Setup(s => s.SendAsync<BioTimeResponseDTO>(It.IsAny<APIRequest>()))
                        .ReturnsAsync(attendanceResponse);

        var connector = new BioTimeConnector(
            _mockHttpService.Object,
            _mockUnitOfWork.Object,
            _mockOptions,
            _mockLogger.Object
        );

        var result = await connector.FetchAttendanceDataAsync();

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("EMP001", result[0].EmployeeCode);
        Assert.Equal("attendance-in", result[0].Function);
    }

    [Fact]
    public async Task FetchAttendanceDataAsync_ReturnsEmpty_WhenTokenFails()
    {
        var tokenResponse = new ApiResponse<TokenDTO>
        {
            IsSuccess = false,
            ErrorMessage = "Invalid credentials"
        };

        _mockHttpService.Setup(s => s.SendAsync<TokenDTO>(It.IsAny<APIRequest>()))
                        .ReturnsAsync(tokenResponse);

        var connector = new BioTimeConnector(
            _mockHttpService.Object,
            _mockUnitOfWork.Object,
            _mockOptions,
            _mockLogger.Object
        );

        var result = await connector.FetchAttendanceDataAsync();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task FetchAttendanceDataAsync_HandlesEmptyPages()
    {
        var tokenResponse = new ApiResponse<TokenDTO>
        {
            IsSuccess = true,
            Data = new TokenDTO { AccessToken = "token" }
        };

        var emptyPageResponse = new ApiResponse<BioTimeResponseDTO>
        {
            IsSuccess = true,
            Data = new BioTimeResponseDTO
            {
                BioTimePunches = new List<BioTimePunches>(),
                NextUrl = null
            }
        };

        _mockHttpService.Setup(s => s.SendAsync<TokenDTO>(It.IsAny<APIRequest>()))
                        .ReturnsAsync(tokenResponse);

        _mockHttpService.Setup(s => s.SendAsync<BioTimeResponseDTO>(It.IsAny<APIRequest>()))
                        .ReturnsAsync(emptyPageResponse);

        var connector = new BioTimeConnector(
            _mockHttpService.Object,
            _mockUnitOfWork.Object,
            _mockOptions,
            _mockLogger.Object
        );

        var result = await connector.FetchAttendanceDataAsync();

        Assert.NotNull(result);
        Assert.Empty(result);
    }
}