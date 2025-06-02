using KayanHRAttendanceService.Application.Interfaces;
using KayanHRAttendanceService.Domain.Entities.Sqlite;
using KayanHRAttendanceService.Domain.Interfaces;
using KayanHRAttendanceService.Infrastructure.Services.AttendanceConnectors.BioTime.DTO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KayanHRAttendanceService.Infrastructure.Services.AttendanceConnectors.BioTime;

public class BioTimeConnector : AttendanceConnector, IAttendanceConnector
{
    private readonly IHttpService _httpService;
    private readonly ILogger<BioTimeConnector> _logger;
    private readonly string _server;
    private readonly string _username;
    private readonly string _password;
    private readonly string _startDate;
    private readonly string _endDate;
    private readonly int _pageSize;

    public BioTimeConnector(IHttpService httpService, IConfiguration configuration, ILogger<BioTimeConnector> logger)
    {
        _httpService = httpService;
        _logger = logger;

        _server = configuration["Integration:Server"] ?? throw new ArgumentNullException("Integration:Server");
        _username = configuration["Integration:Username"] ?? throw new ArgumentNullException("Integration:Username");
        _password = configuration["Integration:Password"] ?? throw new ArgumentNullException("Integration:Password");

        if (!DateTime.TryParse(configuration["Integration:Start_Date"], out var start))
            start = DateTime.UtcNow.AddDays(-1);
        if (!DateTime.TryParse(configuration["Integration:End_Date"], out var end))
            end = DateTime.UtcNow;

        _startDate = start.ToString("yyyy-MM-dd");
        _endDate = end.ToString("yyyy-MM-dd");

        if (!int.TryParse(configuration["Integration:Page_Size"], out _pageSize))
        {
            _pageSize = 100;
            _logger.LogWarning("Invalid or missing 'Page_Size' config. Defaulting to 100.");
        }
    }

    public async Task<List<AttendanceRecord>> FetchAttendanceDataAsync()
    {
        _logger.LogInformation("Fetching attendance data from BioTime from {Start} to {End}", _startDate, _endDate);
        var punches = new List<AttendanceRecord>();
        var token = await AuthenticateAsync();

        int page = 1;
        while (true)
        {
            string url = $"{_server}/iclock/api/transactions/?page={page}&page_size={_pageSize}&start_time={_startDate}&end_time={_endDate}";
            _logger.LogDebug("Requesting page {Page} from BioTime API", page);

            var response = await _httpService.SendAsync<List<BioTimeResponseDTO>>(new Domain.Entities.Services.APIRequest
            {
                Method = HttpMethod.Get,
                Token = token,
                Url = url
            });

            if (response == null || response.Count == 0)
            {
                _logger.LogInformation("No more data found. Stopping at page {Page}", page);
                break;
            }

            punches.AddRange(response.ConvertAll(r => new AttendanceRecord
            {
                TId = r.ID ?? string.Empty,
                EmployeeCode = r.EmployeeCode ?? string.Empty,
                PunchTime = r.PunchTime ?? string.Empty,
                Function = r.PunchStatus ?? string.Empty,
                MachineName = r.MachineName ?? string.Empty,
                MachineSerialNo = r.MachineSerialNo ?? string.Empty
            }));

            page++;
        }

        _logger.LogInformation("Fetched {Count} attendance records", punches.Count);
        return punches;
    }

    public async Task<string> AuthenticateAsync()
    {
        _logger.LogDebug("Authenticating with BioTime server: {Server}", _server);
        var response = await _httpService.SendAsync<TokenDTO>(new Domain.Entities.Services.APIRequest
        {
            Method = HttpMethod.Post,
            Url = $"{_server}/jwt-api-token-auth/",
            Data = new { username = _username, password = _password }
        });

        if (response == null || string.IsNullOrEmpty(response.AccessToken))
        {
            _logger.LogError("Authentication failed: token is null or empty");
            throw new Exception("Authentication failed: token response is null or empty");
        }

        return response.AccessToken;
    }
}
