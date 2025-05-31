//using KayanHRAttendanceService.Entities.Sqlite;
//using KayanHRAttendanceService.WindowsService.Services.IServices;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Logging;
//using Newtonsoft.Json;
//using System.Text.Json;

//namespace KayanHRAttendanceService.WindowsService.AttendanceConnector.BioTime;

//public class BioTimeConnector : AttendanceConnector, IAttendanceConnector
//{
//    private readonly IHttpService _httpService;
//    private readonly ILogger<BioTimeConnector> _logger;
//    private readonly string _server;
//    private readonly string _username;
//    private readonly string _password;
//    private readonly string _startDate;
//    private readonly string _endDate;
//    private readonly int _pageSize;

//    public BioTimeConnector(IHttpService httpService, IConfiguration configuration, ILogger<BioTimeConnector> logger)
//    {
//        _httpService = httpService;
//        _logger = logger;

//        _server = configuration["Integration:Server"] ?? throw new ArgumentNullException("Integration:Server");
//        _username = configuration["Integration:Username"] ?? throw new ArgumentNullException("Integration:Username");
//        _password = configuration["Integration:Password"] ?? throw new ArgumentNullException("Integration:Password");
//        _startDate = configuration["Integration:Start_Date"] ?? DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd");
//        _endDate = configuration["Integration:End_Date"] ?? DateTime.UtcNow.ToString("yyyy-MM-dd");
//        if (!int.TryParse(configuration["Integration:Page_Size"], out _pageSize))
//        {
//            _pageSize = 100;
//            _logger.LogWarning("Invalid or missing 'Page_Size' config. Defaulting to 100.");
//        }
//    }
//    public async Task<List<AttendanceRecord>> FetchAttendanceAsync()
//    {
//        var punches = new List<AttendanceRecord>();
//        var token = await AuthenticateAsync();

//        int page = 1;

//        while (true)
//        {
//            var response = await _httpService.SendAsync<List<BioTimeResponseDTO>>(new Entities.APIRequest
//            {
//                Method = HttpMethod.Get,
//                Token = token,
//                Url = $"{_server}/iclock/api/transactions/?page={page}&page_size={_pageSize}&start_time={_startDate}&end_time={_endDate}"
//            });

//            if (response == null || response.Count == 0)
//                break;

//            punches.AddRange(response.ConvertAll(r => new Storage.Models.AttendanceRecord
//            {
//                TId = r.ID ?? string.Empty,
//                EmployeeCode = r.EmployeeCode ?? string.Empty,
//                PunchTime = r.PunchTime ?? string.Empty,
//                Function = r.PunchStatus ?? string.Empty,
//                MachineName = r.MachineName ?? string.Empty,
//                MachineSerialNo = r.MachineSerialNo ?? string.Empty
//            }));

//            page++;
//        }

//        return punches;
//    }
//    private async Task<string> AuthenticateAsync()
//    {
//        var response = await _httpService.SendAsync<TokenDTO>(new Entities.APIRequest
//        {
//            Method = HttpMethod.Post,
//            Url = $"{_server}/jwt-api-token-auth/",
//            Data = new { _username, _password }
//        });

//        return response.AccessToken;
//    }
//}
//record BioTimeResponseDTO
//{
//    [JsonProperty("id")]
//    public string ID { get; set; }
//    [JsonProperty("emp_code")]
//    public string EmployeeCode { get; set; }
//    [JsonProperty("punch_time")]
//    public string PunchTime { get; set; }
//    [JsonProperty("terminal_alias")]
//    public string MachineName { get; set; }
//    [JsonProperty("terminal_sn")]
//    public string MachineSerialNo { get; set; }
//    [JsonProperty("punch_state")]
//    public string PunchStatus { get; set; }
//}
//record TokenDTO
//{
//    [JsonProperty("token")]
//    public string AccessToken { get; set; }
//}