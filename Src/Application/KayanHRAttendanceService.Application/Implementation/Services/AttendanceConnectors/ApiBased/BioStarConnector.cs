using KayanHRAttendanceService.Application.DTO;
using KayanHRAttendanceService.Application.Interfaces;
using KayanHRAttendanceService.Application.Interfaces.Data;
using KayanHRAttendanceService.Application.Interfaces.Services.AttendanceConnectors;
using KayanHRAttendanceService.Domain.Entities.General;
using KayanHRAttendanceService.Domain.Entities.Services;
using KayanHRAttendanceService.Domain.Entities.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace KayanHRAttendanceService.Application.Implementation.Services.AttendanceConnectors.ApiBased;

public class BioStarConnector(IHttpService httpService, IUnitOfWork unitOfWork, IOptions<IntegrationSettings> settingsOptions, ILogger<BioStarConnector> logger) : ApiBased(unitOfWork, settingsOptions), IAttendanceConnector
{
    private readonly IntegrationSettings _settings = settingsOptions.Value;

    private readonly string _sessionHeaderKey = "bs-session-id";

    public async Task<List<AttendanceRecord>> FetchAttendanceDataAsync()
    {
        logger.LogInformation("Fetching attendance data from BioStar from {Start} to {End}", _settings.StartDate, _settings.EndDate);

        var sessionId = await GetSessionIdAsync();

        if (string.IsNullOrWhiteSpace(sessionId))
            return [];

        var fromTimeStr = await DetermineStartTimeAsync();
        var fromTime = DateTime.Parse(fromTimeStr, null, DateTimeStyles.AdjustToUniversal);

        var records = new List<AttendanceRecord>();

        while (true)
        {
            var batch = await FetchAttendanceBatchAsync(sessionId, fromTime);

            if (batch is null || batch.Count == 0)
                break;

            foreach (var item in batch)
            {
                var punchTime = item.Datetime;

                if (string.IsNullOrEmpty(punchTime))
                    continue;

                var function = item.Event_Type_Id?.Code ?? item.Tna_Key;

                records.Add(new AttendanceRecord
                {
                    TId = item.Index.ToString() ?? "",
                    EmployeeCode = item.User_Id?.User_Id ?? "",
                    PunchTime = punchTime ?? "",
                    Function = MapFunction(function),
                    MachineName = item.Device_Id?.Name ?? "",
                });

                logger.LogInformation("Fetched punch at {PunchTime}", punchTime);
            }

            if (batch.Count < int.Parse(_settings.PageSize))
                break;

            fromTime = DateTime.Parse(batch.Last().Datetime ?? fromTime.ToString("O"));
        }

        return records;
    }

    private async Task<List<BioStarEventDTO>?> FetchAttendanceBatchAsync(string sessionId, DateTime fromTime)
    {
        var isoDate = fromTime.ToString("yyyy-MM-ddTHH:mm:ssZ");

        var payload = new
        {
            Query = new
            {
                limit = _settings.PageSize,
                conditions = new object[] { new { column = "datetime", @operator = 5, values = new[] { isoDate } }, new { column = "user_id", @operator = 1, values = new[] { new { user_id = string.Empty } } } },
                orders = new[] { new { column = "datetime", descending = false } }
            }
        };

        var response = await httpService.SendAsync<BioStarEventSearchResponseDTO>(new APIRequest
        {
            Method = HttpMethod.Post,
            Url = $"{_settings.Server}/api/events/search",
            Data = payload,
            RequestContentType = HttpServiceContentTypes.application_json,
            ResponseContentType = HttpServiceContentTypes.application_json,
            CustomHeaders = new Dictionary<string, string> { { _sessionHeaderKey, sessionId } }
        });

        if (!response.IsSuccess)
        {
            logger.LogError("Failed to fetch events: HTTP {StatusCode} - {Error}", response.StatusCode, response.ErrorMessage);
            return null;
        }

        return response.Data?.EventCollection.Rows;
    }

    private async Task<string?> GetSessionIdAsync()
    {
        var response = await httpService.SendAsync<object>(new APIRequest
        {
            Method = HttpMethod.Post,
            Url = $"{_settings.Server}/api/login",
            Data = new { User = new { login_id = _settings.Username, password = _settings.Password } },
            RequestContentType = HttpServiceContentTypes.application_json,
            ResponseContentType = HttpServiceContentTypes.application_json
        });

        if (!response.IsSuccess)
        {
            logger.LogError("Authentication failed: HTTP {StatusCode}. Error: {Error}", response.StatusCode, response.ErrorMessage ?? "Unknown error");
            return null;
        }

        if (!response.Headers.TryGetValue(_sessionHeaderKey, out var sessionId) || string.IsNullOrWhiteSpace(sessionId))
        {
            logger.LogError("Authentication failed: 'bs-session-id' header missing in response.");
            return null;
        }

        logger.LogInformation("Successfully authenticated and retrieved session ID.");
        return sessionId;
    }
}