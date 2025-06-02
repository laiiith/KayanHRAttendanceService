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
}
