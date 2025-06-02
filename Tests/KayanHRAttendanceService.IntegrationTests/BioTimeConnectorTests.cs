using KayanHRAttendanceService.Application.Interfaces;
using KayanHRAttendanceService.Domain.Entities.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace KayanHRAttendanceService.IntegrationTests;

public class BioTimeConnectorTests
{
    private readonly Mock<IHttpService> _httpServiceMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ILogger<BioTimeConnector>> _loggerMock;
    private readonly BioTimeConnector _bioTimeConnector;

    public BioTimeConnectorTests()
    {
        _httpServiceMock = new Mock<IHttpService>();
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<BioTimeConnector>>();

        _configurationMock.Setup(x => x["Integration:Server"]).Returns("http://testserver.com");
        _configurationMock.Setup(x => x["Integration:Username"]).Returns("testuser");
        _configurationMock.Setup(x => x["Integration:Password"]).Returns("testpass");
        _configurationMock.Setup(x => x["Integration:Start_Date"]).Returns("2023-01-01");
        _configurationMock.Setup(x => x["Integration:End_Date"]).Returns("2023-01-02");
        _configurationMock.Setup(x => x["Integration:Page_Size"]).Returns("100");

        _bioTimeConnector = new BioTimeConnector(
            _httpServiceMock.Object,
            _configurationMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldReturnToken_WhenCredentialsAreValid()
    {
        var expectedToken = "test-token-123";
        _httpServiceMock.Setup(x => x.SendAsync<TokenDTO>(It.IsAny<APIRequest>(), It.IsAny<bool>()))
            .ReturnsAsync(new TokenDTO { AccessToken = expectedToken });

        var result = await _bioTimeConnector.AuthenticateAsync();

        Assert.Equal(expectedToken, result);
        _httpServiceMock.Verify(x => x.SendAsync<TokenDTO>(It.IsAny<APIRequest>(), It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task FetchAttendanceDataAsync_ShouldReturnAttendanceRecords_WhenDataExists()
    {
        var expectedToken = "test-token-123";
        _httpServiceMock.Setup(x => x.SendAsync<TokenDTO>(It.IsAny<APIRequest>(), It.IsAny<bool>()))
            .ReturnsAsync(new TokenDTO { AccessToken = expectedToken });

        var testData = new List<BioTimeResponseDTO>
    {
        new BioTimeResponseDTO
        {
            ID = "1",
            EmployeeCode = "EMP001",
            PunchTime = "2023-01-01 08:00:00",
            PunchStatus = "IN",
            MachineName = "Device1",
            MachineSerialNo = "SN123"
        }
    };

        _httpServiceMock.SetupSequence(x => x.SendAsync<List<BioTimeResponseDTO>>(It.IsAny<APIRequest>(), It.IsAny<bool>()))
            .ReturnsAsync(testData)
            .ReturnsAsync(new List<BioTimeResponseDTO>());

        var result = await _bioTimeConnector.FetchAttendanceDataAsync();

        Assert.Single(result);
        Assert.Equal("EMP001", result[0].EmployeeCode);
        Assert.Equal("2023-01-01 08:00:00", result[0].PunchTime);
        _httpServiceMock.Verify(x => x.SendAsync<List<BioTimeResponseDTO>>(It.IsAny<APIRequest>(), It.IsAny<bool>()), Times.Exactly(2));
    }

    [Fact]
    public async Task FetchAttendanceDataAsync_ShouldHandleEmptyResponse()
    {
        _httpServiceMock.Setup(x => x.SendAsync<TokenDTO>(It.IsAny<APIRequest>(), It.IsAny<bool>()))
            .ReturnsAsync(new TokenDTO { AccessToken = "test-token-123" });

        _httpServiceMock.Setup(x => x.SendAsync<List<BioTimeResponseDTO>>(It.IsAny<APIRequest>(), It.IsAny<bool>()))
            .ReturnsAsync(new List<BioTimeResponseDTO>());

        var result = await _bioTimeConnector.FetchAttendanceDataAsync();

        Assert.Empty(result);
        _httpServiceMock.Verify(x => x.SendAsync<List<BioTimeResponseDTO>>(It.IsAny<APIRequest>(), It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task FetchAttendanceDataAsync_ShouldHandleNullResponse()
    {
        _httpServiceMock.Setup(x => x.SendAsync<TokenDTO>(It.IsAny<APIRequest>(), It.IsAny<bool>()))
            .ReturnsAsync(new TokenDTO { AccessToken = "test-token-123" });

        _httpServiceMock.Setup(x => x.SendAsync<List<BioTimeResponseDTO>>(It.IsAny<APIRequest>(), It.IsAny<bool>()))
            .ReturnsAsync((List<BioTimeResponseDTO>)null);

        var result = await _bioTimeConnector.FetchAttendanceDataAsync();

        Assert.Empty(result);
        _httpServiceMock.Verify(x => x.SendAsync<List<BioTimeResponseDTO>>(It.IsAny<APIRequest>(), It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task FetchAttendanceDataAsync_ShouldMapAllPropertiesCorrectly()
    {
        _httpServiceMock.Setup(x => x.SendAsync<TokenDTO>(It.IsAny<APIRequest>(), It.IsAny<bool>()))
            .ReturnsAsync(new TokenDTO { AccessToken = "test-token-123" });

        var testData = new List<BioTimeResponseDTO>
    {
        new BioTimeResponseDTO
        {
            ID = "1",
            EmployeeCode = "EMP001",
            PunchTime = "2023-01-01 08:00:00",
            PunchStatus = "IN",
            MachineName = "Device1",
            MachineSerialNo = "SN123"
        }
    };

        _httpServiceMock.SetupSequence(x => x.SendAsync<List<BioTimeResponseDTO>>(It.IsAny<APIRequest>(), It.IsAny<bool>()))
            .ReturnsAsync(testData)
            .ReturnsAsync(new List<BioTimeResponseDTO>());

        var result = await _bioTimeConnector.FetchAttendanceDataAsync();

        var record = result.First();
        Assert.Equal("1", record.TId);
        Assert.Equal("EMP001", record.EmployeeCode);
        Assert.Equal("2023-01-01 08:00:00", record.PunchTime);
        Assert.Equal("IN", record.Function);
        Assert.Equal("Device1", record.MachineName);
        Assert.Equal("SN123", record.MachineSerialNo);
    }
}