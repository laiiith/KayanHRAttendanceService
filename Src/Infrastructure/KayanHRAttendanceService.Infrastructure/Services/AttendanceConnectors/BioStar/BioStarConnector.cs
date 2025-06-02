using System.Globalization;
using System.Text.Json;
using KayanHRAttendanceService.Application.Interfaces;
using KayanHRAttendanceService.Domain.Entities.Services;
using KayanHRAttendanceService.Domain.Entities.Sqlite;
using KayanHRAttendanceService.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KayanHRAttendanceService.Infrastructure.Services.AttendanceConnectors.BioStar;

public interface IAttendanceStorage
{
    Task<string?> GetLastPunchTimeAsync();
}
public class BioStarConnector : AttendanceConnector, IAttendanceConnector
{
    private readonly IHttpService _httpService;
    private readonly ILogger<BioStarConnector> _logger;
    private readonly string _server;
    private readonly string _username;
    private readonly string _password;
    private readonly int _limit;
    private readonly string _startDate;
    private readonly bool _dynamicDate;
    private readonly IAttendanceStorage _storage;

    public BioStarConnector(
        IHttpService httpService,
        IConfiguration configuration,
        ILogger<BioStarConnector> logger,
        IAttendanceStorage storage)
    {
        _httpService = httpService;
        _logger = logger;
        _storage = storage;

        _server = configuration["Integration:Server"] ?? throw new ArgumentNullException("Integration:Server");
        _username = configuration["Integration:Username"] ?? throw new ArgumentNullException("Integration:Username");
        _password = configuration["Integration:Password"] ?? throw new ArgumentNullException("Integration:Password");
        _startDate = configuration["Integration:Start_Date"] ?? DateTime.UtcNow.ToString("yyyy-MM-dd");
        _dynamicDate = bool.TryParse(configuration["Integration:Dynamic_Date"], out var dyn) && dyn;

        if (!int.TryParse(configuration["Integration:Page_Size"], out _limit))
        {
            _limit = 100;
            _logger.LogWarning("Invalid or missing 'Page_Size'. Defaulting to 100.");
        }
    }

    public async Task<List<AttendanceRecord>> FetchAttendanceDataAsync()
    {
        var records = new List<AttendanceRecord>();
        string fromDate = _startDate;

        if (_dynamicDate)
        {
            string? lastPunch = await _storage.GetLastPunchTimeAsync();
            if (!string.IsNullOrWhiteSpace(lastPunch))
            {
                fromDate = lastPunch;
                _logger.LogInformation("Dynamic date enabled. Using last punch time: {Time}", fromDate);
            }
            else
            {
                _logger.LogInformation("DB empty. Using config start_date: {Start}", fromDate);
            }
        }

        while (true)
        {
            var batch = await RequestBatchAsync(fromDate);
            if (batch == null || batch.Count == 0)
                break;

            foreach (var item in batch)
            {
                string? functionCode = null;
                if (item.TryGetProperty("event_type_id", out var eventType) && eventType.TryGetProperty("code", out var codeProp))
                {
                    functionCode = codeProp.GetString();
                }
                else if (item.TryGetProperty("tna_key", out var tnaKey))
                {
                    functionCode = tnaKey.GetString();
                }

                records.Add(new AttendanceRecord
                {
                    TId = item.GetProperty("index").ToString(),
                    EmployeeCode = item.GetProperty("user_id").GetProperty("user_id").GetString() ?? string.Empty,
                    PunchTime = item.GetProperty("datetime").GetString() ?? string.Empty,
                    Function = functionCode ?? string.Empty,
                    MachineName = item.GetProperty("device_id").GetProperty("name").GetString() ?? string.Empty,
                    MachineSerialNo = string.Empty
                });

                _logger.LogInformation("Fetched Punch: {Tid}", item.GetProperty("index").ToString());
            }

            if (batch.Count < _limit)
                break;

            fromDate = batch.Last().GetProperty("datetime").GetString() ?? fromDate;
        }

        return records;
    }

    public Task<string?> GetLastPunchTimeAsync()
    {
        throw new NotImplementedException();
    }

    private async Task<string> AuthenticateAsync()
    {
        var payload = new
        {
            User = new
            {
                login_id = _username,
                password = _password
            }
        };


        var response = await _httpService.SendAsync<object>(new APIRequest
        {
            Method = HttpMethod.Post,
            Url = $"{_server}/api/login",
            Data = payload
        });


        throw new NotImplementedException("يجب تعديل طريقة AuthenticateAsync للحصول على 'bs-session-id' من رأس الاستجابة.");
    }

    private async Task<List<JsonElement>> RequestBatchAsync(string dateStr)
    {
        string iso = DateTime.Parse(dateStr).ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
        string sessionId = await AuthenticateAsync();

        var body = new
        {
            Query = new
            {
                limit = _limit,
                conditions = new object[]
                  {
                      new { column = "datetime", @operator = 5, values = new string[] { iso } },
                      new { column = "user_id", @operator = 1, values = new object[] { new { user_id = "" } } }
                  },
                orders = new[]
                {
                    new { column = "datetime", descending = false }
                }
            }
        };

        var response = await _httpService.SendAsync<JsonElement>(new APIRequest
        {
            Method = HttpMethod.Post,
            Url = $"{_server}/api/events/search",
            Data = body,

        });

        if (response.ValueKind == JsonValueKind.Object &&
            response.TryGetProperty("EventCollection", out var eventCollection) &&
            eventCollection.TryGetProperty("rows", out var rows) &&
            rows.ValueKind == JsonValueKind.Array)
        {
            return rows.EnumerateArray().ToList();
        }

        _logger.LogWarning("No rows returned from BioStar search.");
        return new List<JsonElement>();
    }
}
