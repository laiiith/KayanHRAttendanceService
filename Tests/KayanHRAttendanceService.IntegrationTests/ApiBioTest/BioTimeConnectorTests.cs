using KayanHRAttendanceService.Application.DTO;
using KayanHRAttendanceService.Application.Implementation.Services.AttendanceConnectors.ApiBased;
using KayanHRAttendanceService.Application.Interfaces;
using KayanHRAttendanceService.Application.Interfaces.Data;
using KayanHRAttendanceService.Domain.Entities.General;
using KayanHRAttendanceService.Domain.Entities.Services;
using KayanHRAttendanceService.Domain.Entities.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Linq.Expressions;

namespace KayanHRAttendanceService.IntegrationTests.ApiBioTest
{

    public class BioTimeConnectorTests
    {
        private readonly Mock<IHttpService> _httpServiceMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IOptions<IntegrationSettings>> _settingsMock;
        private readonly Mock<ILogger<BioTimeConnector>> _loggerMock;
        private readonly BioTimeConnector _mockedConnector;

        public BioTimeConnectorTests()
        {
            _httpServiceMock = new Mock<IHttpService>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _settingsMock = new Mock<IOptions<IntegrationSettings>>();
            _loggerMock = new Mock<ILogger<BioTimeConnector>>();

            var realSettings = Options.Create(new IntegrationSettings
            {
                Server = "http://biotimedxb.com:8007",
                Username = "admin",
                Password = "admin",
                StartDate = "2023-01-01",
                EndDate = "2023-01-02",
                PageSize = "100",
                DynamicDate = false,
                ConnectionString = "Data Source=test.db",
                GetDataProcedure = "GetAttendanceData",
                UpdateProcedure = "UpdateSyncDate"
            });

            _mockedConnector = new BioTimeConnector(
                _httpServiceMock.Object,
                _unitOfWorkMock.Object,
                realSettings,
                _loggerMock.Object);

        }

        [Fact]
        public async Task FetchAttendanceDataAsync_ShouldReturnEmptyList_WhenTokenIsNull()
        {
            _httpServiceMock
                .Setup(s => s.SendAsync<TokenDTO>(It.IsAny<APIRequest>()))
                .ReturnsAsync(new ApiResponse<TokenDTO> { IsSuccess = false });

            var result = await _mockedConnector.FetchAttendanceDataAsync();

            Assert.Empty(result);
        }

        [Fact]
        public async Task FetchAttendanceDataAsync_ShouldReturnMappedRecords_WhenApiReturnsData()
        {
            SetupValidToken();

            var responseDto = new BioTimeResponseDTO
            {
                BioTimePunches = new List<BioTimePunches>
                {
                    new BioTimePunches
                    {
                        ID = "1",
                        EmployeeCode = "EMP001",
                        PunchTime = "2023-01-01 08:00:00",
                        PunchStatus = "IN",
                        MachineName = "MainGate",
                        MachineSerialNo = "SN001"
                    }
                }
            };

            _httpServiceMock
                .SetupSequence(s => s.SendAsync<BioTimeResponseDTO>(It.IsAny<APIRequest>()))
                .ReturnsAsync(new ApiResponse<BioTimeResponseDTO> { IsSuccess = true, Data = responseDto })
                .ReturnsAsync(new ApiResponse<BioTimeResponseDTO> { IsSuccess = true, Data = new BioTimeResponseDTO { BioTimePunches = new List<BioTimePunches>() } });

            var result = await _mockedConnector.FetchAttendanceDataAsync();

            Assert.Single(result);
            Assert.Equal("EMP001", result[0].EmployeeCode);
            Assert.Equal("IN", result[0].Function);
            Assert.Equal("SN001", result[0].MachineSerialNo);
        }

        private void SetupValidToken()
        {
            _httpServiceMock
                .Setup(s => s.SendAsync<TokenDTO>(It.IsAny<APIRequest>()))
                .ReturnsAsync(new ApiResponse<TokenDTO>
                {
                    IsSuccess = true,
                    Data = new TokenDTO { AccessToken = "valid-token" }
                });
        }

        [Fact]
        public async Task FetchAttendanceDataAsync_WithMockedRealConnection_ShouldReturnMockedData()
        {
            _httpServiceMock
                .SetupSequence(s => s.SendAsync<TokenDTO>(It.IsAny<APIRequest>()))
                .ReturnsAsync(new ApiResponse<TokenDTO>
                {
                    IsSuccess = true,
                    Data = new TokenDTO { AccessToken = "mocked-real-token" }
                });

            _httpServiceMock
                .SetupSequence(s => s.SendAsync<BioTimeResponseDTO>(It.IsAny<APIRequest>()))
                .ReturnsAsync(new ApiResponse<BioTimeResponseDTO>
                {
                    IsSuccess = true,
                    Data = new BioTimeResponseDTO
                    {
                        BioTimePunches = new List<BioTimePunches>
                        {
                            new BioTimePunches
                            {
                                ID = "100",
                                EmployeeCode = "EMP100",
                                PunchTime = "2023-01-05 09:00:00",
                                PunchStatus = "IN",
                                MachineName = "Gate1",
                                MachineSerialNo = "SN100"
                            },
                            new BioTimePunches
                            {
                                ID = "101",
                                EmployeeCode = "EMP101",
                                PunchTime = "2023-01-05 09:15:00",
                                PunchStatus = "IN",
                                MachineName = "Gate2",
                                MachineSerialNo = "SN101"
                            }
                        }
                    }
                })
                .ReturnsAsync(new ApiResponse<BioTimeResponseDTO>
                {
                    IsSuccess = true,
                    Data = new BioTimeResponseDTO
                    {
                        BioTimePunches = new List<BioTimePunches>()
                    }
                });

            var result = await _mockedConnector.FetchAttendanceDataAsync();

            Assert.NotNull(result);
            Assert.True(result.Count == 2);
            Assert.Contains(result, r => r.EmployeeCode == "EMP100");
            Assert.Contains(result, r => r.EmployeeCode == "EMP101");
        }



        private class RealHttpService : IHttpService
        {
            private readonly HttpClient _httpClient = new();

            async Task<ApiResponse<TResponse>> IHttpService.SendAsync<TResponse>(APIRequest request)
            {
                var response = await _httpClient.GetAsync(request.Url);
                if (!response.IsSuccessStatusCode)
                    return new ApiResponse<TResponse> { IsSuccess = false };

                var content = await response.Content.ReadAsStringAsync();
                var data = System.Text.Json.JsonSerializer.Deserialize<TResponse>(
                    content,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return new ApiResponse<TResponse> { IsSuccess = true, Data = data };
            }
        }

        private class RealUnitOfWork : IUnitOfWork
        {
            public IAttendanceDataRepository AttendanceData { get; }

            public RealUnitOfWork()
            {
                AttendanceData = new RealAttendanceDataRepository();
            }

            public Task<int> CompleteAsync(CancellationToken cancellationToken = default)
            {
                return Task.FromResult(1);
            }

            public void Dispose() { }

            public Task Save()
            {
                throw new NotImplementedException();
            }
        }

        private class RealAttendanceDataRepository : IAttendanceDataRepository
        {
            public Task AddAsync(AttendanceRecord entity)
            {
                throw new NotImplementedException();
            }

            public Task AddAsync(AttendanceRecord[] entity)
            {
                throw new NotImplementedException();
            }

            public Task<IEnumerable<AttendanceRecord>> GetAllAsync(Expression<Func<AttendanceRecord, bool>>? filter = null)
            {
                throw new NotImplementedException();
            }

            public Task<AttendanceRecord?> GetAsync(Expression<Func<AttendanceRecord, bool>>? filter = null)
            {
                throw new NotImplementedException();
            }

            public Task<string> GetLastPunchTime()
            {
                return Task.FromResult<string>(null);
            }
        }
    }
}
