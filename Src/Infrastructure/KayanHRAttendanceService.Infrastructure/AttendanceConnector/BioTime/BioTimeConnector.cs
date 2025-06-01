using KayanHRAttendanceService.Application.AttendanceConnector.DTO;
using KayanHRAttendanceService.Application.AttendanceConnector.Interfaces;
using KayanHRAttendanceService.Application.Services.Interfaces;
using KayanHRAttendanceService.Domain.Entities.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KayanHRAttendanceService.Infrastructure.AttendanceConnector.BioTime;

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
        _startDate = configuration["Integration:Start_Date"] ?? DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd");
        _endDate = configuration["Integration:End_Date"] ?? DateTime.UtcNow.ToString("yyyy-MM-dd");
        if (!int.TryParse(configuration["Integration:Page_Size"], out _pageSize))
        {
            _pageSize = 100;
            _logger.LogWarning("Invalid or missing 'Page_Size' config. Defaulting to 100.");
        }
    }
    public async Task<List<AttendanceRecord>> FetchAttendanceDataAsync()
    {
        var punches = new List<AttendanceRecord>();
        var token = await AuthenticateAsync();

        int page = 1;

        while (true)
        {
            var response = await _httpService.SendAsync<List<BioTimeResponseDTO>>(new Domain.Entities.General.APIRequest
            {
                Method = HttpMethod.Get,
                Token = token,
                Url = $"{_server}/iclock/api/transactions/?page={page}&page_size={_pageSize}&start_time={_startDate}&end_time={_endDate}"
            });

            if (response == null || response.Count == 0)
                break;

            punches.AddRange(response.ConvertAll(r => new Domain.Entities.Sqlite.AttendanceRecord
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

        return punches;
    }
    private async Task<string> AuthenticateAsync()
    {
        var response = await _httpService.SendAsync<TokenDTO>(new Domain.Entities.General.APIRequest
        {
            Method = HttpMethod.Post,
            Url = $"{_server}/jwt-api-token-auth/",
            Data = new { _username, _password }
        });

        return response.AccessToken;
    }
}
