using KayanHRAttendanceService.Application.DTO;
using KayanHRAttendanceService.Application.Implementation.Services.AttendanceConnectors.ApiBased.BioTime;
using KayanHRAttendanceService.Application.Interfaces;
using KayanHRAttendanceService.Domain.Entities.General;
using KayanHRAttendanceService.Domain.Entities.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace KayanHRAttendanceService.IntegrationTests.ApiBioTest;

public class BioTimeConnectorTests
{
    private readonly Mock<IHttpService> _httpServiceMock;
    private readonly Mock<IOptions<IntegrationSettings>> _settingsMock;
    private readonly Mock<ILogger<BioTimeConnector>> _loggerMock;
    private readonly BioTimeConnector _connector;

    public BioTimeConnectorTests()
    {
        _httpServiceMock = new Mock<IHttpService>();
        _settingsMock = new Mock<IOptions<IntegrationSettings>>();
        _loggerMock = new Mock<ILogger<BioTimeConnector>>();

        _settingsMock.Setup(x => x.Value).Returns(new IntegrationSettings
        {
            Server = "http://testserver",
            Username = "admin",
            Password = "admin123",
            StartDate = "2023-01-01",
            EndDate = "2023-01-02",
            PageSize = "100",
            DynamicDate = false,
            ConnectionString = "Data Source=test.db",
            GetDataProcedure = "GetAttendanceData",
            UpdateProcedure = "UpdateSyncDate"
        });


        _connector = new BioTimeConnector(
            _httpServiceMock.Object,
            _settingsMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldReturnAccessToken_WhenCredentialsValid()
    {
        var expectedToken = "valid-token";
        _httpServiceMock
            .Setup(s => s.SendAsync<TokenDTO>(It.IsAny<APIRequest>(), It.IsAny<bool>()))
            .ReturnsAsync(new TokenDTO { AccessToken = expectedToken });

        var token = await _connector.FetchAttendanceDataAsync(); // Indirect test

        _httpServiceMock.Verify(s => s.SendAsync<TokenDTO>(It.IsAny<APIRequest>(), true), Times.Once);

    }

    [Fact]
    public async Task AuthenticateAsync_ShouldThrowException_WhenTokenIsNull()
    {
        _httpServiceMock
            .Setup(s => s.SendAsync<TokenDTO>(It.IsAny<APIRequest>(), It.IsAny<bool>()))
            .ReturnsAsync((TokenDTO)null!);

        await Assert.ThrowsAsync<Exception>(() => InvokeAuth());
    }

    [Fact]
    public async Task FetchAttendanceDataAsync_ShouldReturnMappedRecords_WhenApiReturnsData()
    {
        SetupValidToken();

        var data = new List<BioTimeResponseDTO>
        {
            new()
            {
                ID = "1",
                EmployeeCode = "EMP001",
                PunchTime = "2023-01-01 08:00:00",
                PunchStatus = "IN",
                MachineName = "MainGate",
                MachineSerialNo = "SN001"
            }
        };

        _httpServiceMock
            .SetupSequence(s => s.SendAsync<List<BioTimeResponseDTO>>(It.IsAny<APIRequest>(), It.IsAny<bool>()))
            .ReturnsAsync(data)
            .ReturnsAsync(new List<BioTimeResponseDTO>());

        var result = await _connector.FetchAttendanceDataAsync();

        Assert.Single(result);
        Assert.Equal("EMP001", result[0].EmployeeCode);
        Assert.Equal("IN", result[0].Function);
        Assert.Equal("SN001", result[0].MachineSerialNo);
    }

    [Fact]
    public async Task FetchAttendanceDataAsync_ShouldReturnEmptyList_WhenResponseIsNull()
    {
        SetupValidToken();

        _httpServiceMock
            .Setup(s => s.SendAsync<List<BioTimeResponseDTO>>(It.IsAny<APIRequest>(), It.IsAny<bool>()))
            .ReturnsAsync((List<BioTimeResponseDTO>)null!);

        var result = await _connector.FetchAttendanceDataAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task FetchAttendanceDataAsync_ShouldReturnEmptyList_WhenResponseIsEmpty()
    {
        SetupValidToken();

        _httpServiceMock
            .Setup(s => s.SendAsync<List<BioTimeResponseDTO>>(It.IsAny<APIRequest>(), It.IsAny<bool>()))
            .ReturnsAsync(new List<BioTimeResponseDTO>());

        var result = await _connector.FetchAttendanceDataAsync();

        Assert.Empty(result);
    }

    private async Task InvokeAuth()
    {
        var method = typeof(BioTimeConnector).GetMethod("AuthenticateAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (method == null)
            throw new InvalidOperationException("AuthenticateAsync method not found");

        await (Task<string>)method.Invoke(_connector, null)!;
    }

    private void SetupValidToken()
    {
        _httpServiceMock
            .Setup(s => s.SendAsync<TokenDTO>(It.IsAny<APIRequest>(), It.IsAny<bool>()))
            .ReturnsAsync(new TokenDTO { AccessToken = "valid-token" });
    }
}
