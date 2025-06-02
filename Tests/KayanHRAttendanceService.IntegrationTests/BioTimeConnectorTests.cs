using KayanHRAttendanceService.Application.Interfaces;
using KayanHRAttendanceService.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace KayanHRAttendanceService.IntegrationTests;

public class BioTimeConnectorTests
{
    private readonly IConfiguration _configuration;
    private readonly Mock<IHttpService> _httpServiceMock;
    private readonly Mock<ILogger<IAttendanceConnector>> _loggerMock;

    public BioTimeConnectorTests()
    {
        var inMemorySettings = new Dictionary<string, string>
        {
            {"Integration:Server", "http://localhost"},
            {"Integration:Username", "admin"},
            {"Integration:Password", "1234"},
            {"Integration:Start_Date", "2024-01-01"},
            {"Integration:End_Date", "2024-01-02"},
            {"Integration:Page_Size", "2"}
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _httpServiceMock = new Mock<IHttpService>();
        _loggerMock = new Mock<ILogger<IAttendanceConnector>>();
    }

    //[Fact]
    //public async Task FetchAttendanceDataAsync_ShouldFetchAndReturnAttendanceRecords()
    //{
    //    var token = "fake-token";

    //    var page1Data = new List<BioTimeResponseDTO>
    //    {
    //        new() { ID = "1", EmployeeCode = "EMP001", PunchTime = "2024-01-01T08:00:00", PunchStatus = "IN", MachineName = "M1", MachineSerialNo = "SN001" },
    //        new() { ID = "2", EmployeeCode = "EMP002", PunchTime = "2024-01-01T09:00:00", PunchStatus = "IN", MachineName = "M2", MachineSerialNo = "SN002" }
    //    };
    //    var emptyPage = new List<BioTimeResponseDTO>();

    //    _httpServiceMock
    //        .Setup(s => s.SendAsync<TokenDTO>(It.Is<APIRequest>(r => r.Url!.Contains("/jwt-api-token-auth/")), true))
    //        .ReturnsAsync(new TokenDTO { AccessToken = token });

    //    _httpServiceMock
    //        .SetupSequence(s => s.SendAsync<List<BioTimeResponseDTO>>(It.Is<APIRequest>(r => r.Url!.Contains("/iclock/api/transactions/")), true))
    //        .ReturnsAsync(page1Data)
    //        .ReturnsAsync(emptyPage);

    //    var connector = new (_httpServiceMock.Object, _configuration, _loggerMock.Object);

    //    var result = await connector.FetchAttendanceDataAsync();

    //    Assert.NotNull(result);
    //    Assert.Equal(2, result.Count);
    //    Assert.Equal("EMP001", result[0].EmployeeCode);
    //    Assert.Equal("EMP002", result[1].EmployeeCode);
    //}
}
