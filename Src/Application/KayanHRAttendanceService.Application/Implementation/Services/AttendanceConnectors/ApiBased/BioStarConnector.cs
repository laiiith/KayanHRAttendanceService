using KayanHRAttendanceService.Application.Interfaces;
using KayanHRAttendanceService.Application.Interfaces.Services.AttendanceConnectors;
using KayanHRAttendanceService.Domain.Entities.General;
using KayanHRAttendanceService.Domain.Entities.Services;
using KayanHRAttendanceService.Domain.Entities.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text.Json;

namespace KayanHRAttendanceService.Application.Implementation.Services.AttendanceConnectors.ApiBased;

public class BioStarConnector(IHttpService httpService, IOptions<IntegrationSettings> settings, ILogger<BioStarConnector> logger) : IAttendanceConnector
{
    public async Task<List<AttendanceRecord>> FetchAttendanceDataAsync()
    {
        logger.LogInformation("Fetching attendance data from BioStar from {Start} to {End}", settings.Value.StartDate, settings.Value.EndDate);

        if (!int.TryParse(settings.Value.PageSize, out int pageSize))
        {
            logger.LogError("Invalid PageSize value: {PageSize}", settings.Value.PageSize);
            throw new Exception("Invalid PageSize value in settings.");
        }

        var records = new List<AttendanceRecord>();

        string fromDate = settings.Value.StartDate!;
        string toDate = settings.Value.EndDate!;
        string sessionId = await AuthenticateAsync();

        int page = 0;
        while (true)
        {
            var batch = await RequestBatchAsync(sessionId, fromDate, toDate, page, pageSize);
            if (batch is null || batch.Count == 0)
            {
                logger.LogInformation("No more data found. Stopping at page {Page}", page);
                break;
            }

            foreach (var item in batch)
            {
                string? functionCode = null;

                if (item.TryGetProperty("event_type_id", out var eventType) &&
                    eventType.TryGetProperty("code", out var codeProp))
                {
                    functionCode = codeProp.GetString();
                }
                else if (item.TryGetProperty("tna_key", out var tnaKey))
                {
                    functionCode = tnaKey.GetString();
                }

                string employeeCode = string.Empty;
                if (item.TryGetProperty("user_id", out var userIdProp) &&
                    userIdProp.TryGetProperty("user_id", out var userCodeProp))
                {
                    employeeCode = userCodeProp.GetString() ?? string.Empty;
                }

                string machineName = string.Empty;
                if (item.TryGetProperty("device_id", out var deviceIdProp) &&
                    deviceIdProp.TryGetProperty("name", out var deviceNameProp))
                {
                    machineName = deviceNameProp.GetString() ?? string.Empty;
                }

                records.Add(new AttendanceRecord
                {
                    TId = item.GetProperty("index").ToString(),
                    EmployeeCode = employeeCode.Trim(),
                    PunchTime = item.GetProperty("datetime").GetString() ?? string.Empty,
                    Function = functionCode?.Trim() ?? string.Empty,
                    MachineName = machineName.Trim(),
                    MachineSerialNo = string.Empty // Not provided in API
                });

                logger.LogDebug("Fetched Punch: {Tid}", item.GetProperty("index").ToString());
            }

            if (batch.Count < pageSize)
                break;

            fromDate = batch.Last().GetProperty("datetime").GetString() ?? fromDate;
            page++;
        }

        logger.LogInformation("Fetched {Count} attendance records", records.Count);
        return records;
    }

    private async Task<string> AuthenticateAsync()
    {
        var payload = new
        {
            User = new
            {
                login_id = settings.Value.Username,
                password = settings.Value.Password
            }
        };

        var response = await httpService.SendAsync<JsonElement>(new APIRequest
        {
            Method = HttpMethod.Post,
            Url = $"{settings.Value.Server}/api/login",
            Data = payload
        });

        if (response.Data.ValueKind == JsonValueKind.Object &&
            response.Data.TryGetProperty("SessionID", out var sessionIdProp) &&
            sessionIdProp.ValueKind == JsonValueKind.String)
        {
            return sessionIdProp.GetString()!;
        }

        logger.LogError("Authentication failed: SessionID not found.");
        throw new Exception("Authentication failed.");
    }

    private async Task<List<JsonElement>> RequestBatchAsync(string sessionId, string fromDate, string toDate, int page, int pageSize)
    {
        int offset = page * pageSize;

        if (!DateTime.TryParse(fromDate, out var parsedStart))
        {
            logger.LogError("Invalid fromDate format: {FromDate}", fromDate);
            throw new Exception("Invalid fromDate format.");
        }

        string isoStart = parsedStart.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);

        var body = new
        {
            Query = new
            {
                limit = pageSize,
                offset,
                conditions = new object[]
                {
                    new { column = "datetime", @operator = 5, values = new[] { isoStart } },
                },
                orders = new[]
                {
                    new { column = "datetime", descending = false }
                }
            }
        };

        var response = await httpService.SendAsync<JsonElement>(new APIRequest
        {
            Method = HttpMethod.Post,
            Url = $"{settings.Value.Server}/api/events/search",
            Data = body,
            CustomHeaders = new Dictionary<string, string>
            {
                { "bs-session-id", sessionId }
            }
        });

        if (response.Data.ValueKind == JsonValueKind.Object &&
            response.Data.TryGetProperty("EventCollection", out var eventCollection) &&
            eventCollection.TryGetProperty("rows", out var rows) &&
            rows.ValueKind == JsonValueKind.Array)
        {
            return rows.EnumerateArray().ToList();
        }

        logger.LogWarning("No rows returned from BioStar search.");
        return [];
    }
}