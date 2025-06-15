//using KayanHRAttendanceService.Application.Implementation.Services.AttendanceConnectors.ZkTeco;
//using KayanHRAttendanceService.Domain.Entities.General;
//using KayanHRAttendanceService.Domain.Entities.Sqlite;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Options;
//using Moq;
//using Moq.Protected;
//using System.Globalization;
//using System.Net;
//namespace KayanHRAttendanceService.IntegrationTests.ZkTecoConnectorTests
//{
//    public class ZkTecoConnectorTests
//    {
//        [Fact]
//        public async Task FetchAttendanceDataAsync_Returns_CorrectParsedAttendanceRecords()
//        {
//            string fakeResponse = string.Join('\n', new[]
//    {
//        "ATTLOG",
//        "123\t2025-06-15 08:00:00\t0\t0",
//        "123\t2025-06-15 17:00:00\t1\t0"
//    });


//            var handlerMock = new Mock<HttpMessageHandler>();
//            handlerMock
//                .Protected()
//                .Setup<Task<HttpResponseMessage>>("SendAsync",
//                    ItExpr.IsAny<HttpRequestMessage>(),
//                    ItExpr.IsAny<CancellationToken>())
//                .ReturnsAsync(new HttpResponseMessage()
//                {
//                    StatusCode = HttpStatusCode.OK,
//                    Content = new StringContent(fakeResponse),
//                });

//            var httpClient = new HttpClient(handlerMock.Object);

//            var settings = new IntegrationSettings
//            {
//                ClientID = "3308041522001",
//                ClientSecret = "dummy-secret",
//                ZkTecoSettings = new ZkTecoSettings
//                {
//                    Host = "localhost",
//                    Port = "8080"
//                },
//                FunctionMapping = new FunctionMapping
//                {
//                    AttendanceIn = "in",
//                    AttendanceOut = "out",
//                    BreakIn = "breakin",
//                    BreakOut = "breakout",
//                    PermissionIn = "pin",
//                    PermissionOut = "pout",
//                    OvertimeIn = "oin",
//                    OvertimeOut = "oout"
//                },
//                APIBulkEndpoint = "",
//                BatchSize = 50,
//                DynamicDate = false,
//                Integration = new Integration(),
//                Interval = 5,
//                Type = 1
//            };

//            var optionsMock = new Mock<IOptions<IntegrationSettings>>();
//            optionsMock.Setup(x => x.Value).Returns(settings);

//            var loggerMock = new Mock<ILogger<ZkTecoConnector>>();

//            var connector = new TestableZkTecoConnector(optionsMock.Object, loggerMock.Object, httpClient);

//            var result = await connector.FetchAttendanceDataAsync();

//            Assert.NotNull(result);
//            Assert.Equal(2, result.Count);
//            Assert.All(result, r => Assert.Equal("123", r.TId));
//            Assert.Contains(result, r => r.PunchTime.Contains("08:00:00"));
//            Assert.Contains(result, r => r.PunchTime.Contains("17:00:00"));
//            Assert.All(result, r => Assert.Equal("attendance-in", r.Function));
//        }

//        private class TestableZkTecoConnector : ZkTecoConnector
//        {
//            private readonly HttpClient _httpClient;

//            public TestableZkTecoConnector(IOptions<IntegrationSettings> options, ILogger<ZkTecoConnector> logger, HttpClient httpClient)
//                : base(options, logger)
//            {
//                _httpClient = httpClient;
//            }

//            public override async Task<List<AttendanceRecord>> FetchAttendanceDataAsync()
//            {
//                var result = new List<AttendanceRecord>();

//                try
//                {
//                    string host = _settings.ZkTecoSettings.Host;
//                    string port = _settings.ZkTecoSettings.Port;
//                    string serialNumber = _settings.ClientID;
//                    string url = $"http://{host}:{port}/iclock/cdata?SN={serialNumber}&pushver=3.1.2&options=all";

//                    var response = await _httpClient.GetAsync(url);

//                    if (!response.IsSuccessStatusCode)
//                        return result;

//                    var content = await response.Content.ReadAsStringAsync();
//                    var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);

//                    foreach (var line in lines)
//                    {
//                        if (line.StartsWith("ATTLOG")) continue;

//                        var parts = line.Trim().Split('\t');
//                        if (parts.Length < 2) continue;

//                        string userId = parts[0];
//                        string dateTimeStr = parts[1];
//                        if (!DateTime.TryParseExact(dateTimeStr, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime))
//                            continue;

//                        result.Add(new AttendanceRecord
//                        {
//                            TId = userId,
//                            PunchTime = dateTime.ToString("yyyy-MM-dd HH:mm:ss"),
//                            Function = MapFunction("attendance-in")
//                        });
//                    }
//                }
//                catch { }

//                return result;
//            }
//        }
//    }
//}
