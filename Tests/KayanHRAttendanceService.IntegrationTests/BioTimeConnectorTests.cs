using KayanHRAttendanceService.Application.Interfaces;
using KayanHRAttendanceService.Domain.Entities.General;
using KayanHRAttendanceService.Domain.Entities.Services;
using KayanHRAttendanceService.Infrastructure.Services.AttendanceConnectors.BioTime;
using KayanHRAttendanceService.Infrastructure.Services.AttendanceConnectors.BioTime.DTO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace KayanHRAttendanceService.IntegrationTests
{
    public class BioTimeConnectorTests
    {
        private readonly Mock<IHttpService> _httpServiceMock;
        private readonly Mock<ILogger<BioTimeConnector>> _loggerMock;
        private readonly BioTimeConnector _bioTimeConnector;

        public BioTimeConnectorTests()
        {
            _httpServiceMock = new Mock<IHttpService>();
            _loggerMock = new Mock<ILogger<BioTimeConnector>>();

            var settings = new IntegrationSettings
            {
                Server = "http://testserver.com",
                Username = "testuser",
                Password = "testpass",
                StartDate = "2023-01-01",
                EndDate = "2023-01-02",
                PageSize = "100",

                ConnectionString = "Server=localhost;Database=TestDb;User Id=test;Password=pass;",
                GetDataProcedure = "sp_GetAttendanceData"
            };


            var options = Options.Create(settings);

            _bioTimeConnector = new BioTimeConnector(
                _httpServiceMock.Object,
                options,
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
        }

        [Fact]
        public async Task AuthenticateAsync_ShouldThrowException_WhenTokenIsNull()
        {
            _httpServiceMock.Setup(x => x.SendAsync<TokenDTO>(It.IsAny<APIRequest>(), It.IsAny<bool>()))
                .ReturnsAsync((TokenDTO)null);

            var ex = await Assert.ThrowsAsync<Exception>(() => _bioTimeConnector.AuthenticateAsync());

            Assert.Equal("Authentication failed: token response is null or empty", ex.Message);
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

        [Fact]
        public async Task FetchAttendanceDataAsync_ShouldHandleNullFields()
        {
            _httpServiceMock.Setup(x => x.SendAsync<TokenDTO>(It.IsAny<APIRequest>(), It.IsAny<bool>()))
                .ReturnsAsync(new TokenDTO { AccessToken = "test-token-123" });

            var testData = new List<BioTimeResponseDTO>
            {
                new BioTimeResponseDTO
                {
                    ID = null,
                    EmployeeCode = null,
                    PunchTime = null,
                    PunchStatus = null,
                    MachineName = null,
                    MachineSerialNo = null
                }
            };

            _httpServiceMock.SetupSequence(x => x.SendAsync<List<BioTimeResponseDTO>>(It.IsAny<APIRequest>(), It.IsAny<bool>()))
                .ReturnsAsync(testData)
                .ReturnsAsync(new List<BioTimeResponseDTO>());

            var result = await _bioTimeConnector.FetchAttendanceDataAsync();
            var record = result.First();

            Assert.Equal(string.Empty, record.TId);
            Assert.Equal(string.Empty, record.EmployeeCode);
            Assert.Equal(string.Empty, record.PunchTime);
            Assert.Equal(string.Empty, record.Function);
            Assert.Equal(string.Empty, record.MachineName);
            Assert.Equal(string.Empty, record.MachineSerialNo);
        }

        [Fact]
        public async Task FetchAttendanceDataAsync_ShouldFetchMultiplePages()
        {
            _httpServiceMock.Setup(x => x.SendAsync<TokenDTO>(It.IsAny<APIRequest>(), It.IsAny<bool>()))
                .ReturnsAsync(new TokenDTO { AccessToken = "test-token-123" });

            var page1 = new List<BioTimeResponseDTO>
            {
                new BioTimeResponseDTO { ID = "1", EmployeeCode = "EMP001", PunchTime = "2023-01-01 08:00:00", PunchStatus = "IN" }
            };

            var page2 = new List<BioTimeResponseDTO>
            {
                new BioTimeResponseDTO { ID = "2", EmployeeCode = "EMP002", PunchTime = "2023-01-01 09:00:00", PunchStatus = "OUT" }
            };

            _httpServiceMock.SetupSequence(x => x.SendAsync<List<BioTimeResponseDTO>>(It.IsAny<APIRequest>(), It.IsAny<bool>()))
                .ReturnsAsync(page1)
                .ReturnsAsync(page2)
                .ReturnsAsync(new List<BioTimeResponseDTO>());

            var result = await _bioTimeConnector.FetchAttendanceDataAsync();

            Assert.Equal(2, result.Count);
            Assert.Contains(result, r => r.EmployeeCode == "EMP001");
            Assert.Contains(result, r => r.EmployeeCode == "EMP002");
        }

        [Fact]
        public void Constructor_ShouldUseDefaultPageSize_WhenInvalidPageSizeProvided()
        {
            var settings = new IntegrationSettings
            {
                Server = "http://testserver.com",
                Username = "testuser",
                Password = "testpass",
                StartDate = "2023-01-01",
                EndDate = "2023-01-02",
                PageSize = "invalid",

                ConnectionString = "Server=localhost;Database=TestDb;User Id=test;Password=pass;",
                GetDataProcedure = "sp_GetAttendanceData"
            };

            var options = Options.Create(settings);
            var connector = new BioTimeConnector(new Mock<IHttpService>().Object, options, new Mock<ILogger<BioTimeConnector>>().Object);
        }

    }
}
