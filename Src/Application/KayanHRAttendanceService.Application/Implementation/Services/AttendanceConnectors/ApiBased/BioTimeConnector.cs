using KayanHRAttendanceService.Application.DTO;
using KayanHRAttendanceService.Application.Interfaces;
using KayanHRAttendanceService.Application.Interfaces.Data;
using KayanHRAttendanceService.Application.Interfaces.Services.AttendanceConnectors;
using KayanHRAttendanceService.Domain.Entities.General;
using KayanHRAttendanceService.Domain.Entities.Services;
using KayanHRAttendanceService.Domain.Entities.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KayanHRAttendanceService.Application.Implementation.Services.AttendanceConnectors.ApiBased;

public class BioTimeConnector(IHttpService httpService, IUnitOfWork unitOfWork, IOptions<IntegrationSettings> settings, ILogger<BioTimeConnector> logger) : IAttendanceConnector
{
    public async Task<List<AttendanceRecord>> FetchAttendanceDataAsync()
    {
        var token = await GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            return [];
        }

        var startTime = await DetermineStartTimeAsync();

        return await FetchAllPagesAsync(token, startTime);
    }

    private async Task<List<AttendanceRecord>> FetchAllPagesAsync(string token, string startTime)
    {
        var allRecords = new List<AttendanceRecord>();

        int page = 1;

        while (true)
        {
            var records = await FetchPageAsync(token, page, startTime);
            if (records == null || records.Count == 0)
                break;

            allRecords.AddRange(records);
            page++;
        }

        logger.LogInformation("Total attendance records fetched: {TotalCount}", allRecords.Count);
        return allRecords;
    }

    private async Task<List<AttendanceRecord>?> FetchPageAsync(string token, int page, string startTime)
    {
        var response = await httpService.SendAsync<BioTimeResponseDTO>(new APIRequest
        {
            Url = $"{settings.Value.Server}/iclock/api/transactions/",
            Method = HttpMethod.Get,
            Token = token,
            QueryParameters = new Dictionary<string, string>
                {
                    { "page", page.ToString() },
                    { "page_size", settings.Value.PageSize },
                    { "start_time", startTime },
                    { "end_time", settings.Value.EndDate },
                },
            RequestContentType = HttpServiceContentTypes.application_json,
            ResponseContentType = HttpServiceContentTypes.application_json
        });

        if (!response.IsSuccess)
        {
            logger.LogError("Failed to fetch attendance data on page {Page}. Error: {Error}", page, response.ErrorMessage ?? "Unknown error");
            return null;
        }

        if (response.Data?.BioTimePunches == null || response.Data.BioTimePunches.Count == 0)
        {
            logger.LogInformation("No attendance data found on page {Page}.", page);
            return [];
        }

        return response.Data.BioTimePunches.ConvertAll(r => new AttendanceRecord
        {
            TId = r.ID ?? string.Empty,
            EmployeeCode = r.EmployeeCode ?? string.Empty,
            PunchTime = r.PunchTime ?? string.Empty,
            Function = r.PunchStatus ?? string.Empty,
            MachineName = r.MachineName ?? string.Empty,
            MachineSerialNo = r.MachineSerialNo ?? string.Empty
        });
    }

    private async Task<string> DetermineStartTimeAsync()
    {
        if (!settings.Value.DynamicDate)
            return settings.Value.StartDate;

        var lastPunchTime = await unitOfWork.AttendanceData.GetLastPunchTime();
        return !string.IsNullOrWhiteSpace(lastPunchTime) ? lastPunchTime : settings.Value.StartDate;
    }

    private async Task<string?> GetTokenAsync()
    {
        var response = await httpService.SendAsync<TokenDTO>(new APIRequest
        {
            Method = HttpMethod.Post,
            Url = $"{settings.Value.Server}/jwt-api-token-auth/",
            Data = new
            {
                username = settings.Value.Username,
                password = settings.Value.Password
            },
            RequestContentType = HttpServiceContentTypes.application_json,
            ResponseContentType = HttpServiceContentTypes.application_json
        });

        if (!response.IsSuccess)
        {
            logger.LogError("Authentication failed: {Error}", response.ErrorMessage ?? "Unknown error");
            return null;
        }

        if (string.IsNullOrWhiteSpace(response.Data?.AccessToken))
        {
            logger.LogError("Authentication failed: AccessToken is missing in response");
            return null;
        }

        return response.Data.AccessToken;
    }
}