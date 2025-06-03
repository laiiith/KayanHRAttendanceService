using KayanHRAttendanceService.Application.DTO;
using KayanHRAttendanceService.Application.Implementation.Services.AttendanceConnectors.ApiBased;
using KayanHRAttendanceService.Application.Interfaces;
using KayanHRAttendanceService.Application.Interfaces.Data;
using KayanHRAttendanceService.Domain.Entities.General;
using KayanHRAttendanceService.Domain.Entities.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

public class BioTimeConnectorTests
{
    private readonly Mock<IHttpService> _mockHttpService = new();
    private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();
    private readonly Mock<ILogger<BioTimeConnector>> _mockLogger = new();
    private readonly IOptions<IntegrationSettings> _mockOptions;

    public BioTimeConnectorTests()
    {
        _mockOptions = Options.Create(new IntegrationSettings
        {
            Server = "http://fake-server.com",
            Username = "user",
            Password = "pass",
            StartDate = "2025-01-01",
            EndDate = "2025-01-31",
            PageSize = "100",
            Function_Mapping = new FunctionMapping
            {
                Attendance_In = "0",
                Attendance_Out = "1",
                Break_In = "2",
                Break_Out = "3",
                Permission_In = "4",
                Permission_Out = "5",
                Overtime_In = "6",
                Overtime_Out = "7"
            }
        });

    }

    [Fact]
    public async Task FetchAttendanceDataAsync_ReturnsAttendanceRecords()
    {
        // Arrange
        var tokenResponse = new ApiResponse<TokenDTO>
        {
            IsSuccess = true,
            Data = new TokenDTO { AccessToken = "fake-token" }
        };

        var attendanceData = new List<BioTimePunches>
        {
            new BioTimePunches
            {
                ID = "1",
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
                BioTimePunches = attendanceData
            }
        };

        _mockHttpService.Setup(s => s.SendAsync<TokenDTO>(It.IsAny<APIRequest>()))
                        .ReturnsAsync(tokenResponse);

        _mockHttpService.SetupSequence(s => s.SendAsync<BioTimeResponseDTO>(It.IsAny<APIRequest>()))
                        .ReturnsAsync(attendanceResponse)
                        .ReturnsAsync(new ApiResponse<BioTimeResponseDTO>
                        {
                            IsSuccess = true,
                            Data = new BioTimeResponseDTO
                            {
                                BioTimePunches = new List<BioTimePunches>()
                            }
                        });

        var connector = new BioTimeConnector(
            _mockHttpService.Object,
            _mockUnitOfWork.Object,
            _mockOptions,
            _mockLogger.Object
        );

        // Act
        var result = await connector.FetchAttendanceDataAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("EMP001", result[0].EmployeeCode);
    }
}
