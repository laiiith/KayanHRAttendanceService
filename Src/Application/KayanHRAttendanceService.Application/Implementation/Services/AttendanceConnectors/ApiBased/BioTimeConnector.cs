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

public class BioTimeConnector(IHttpService httpService, IUnitOfWork unitOfWork, IOptions<IntegrationSettings> settingsOptions, ILogger<BioTimeConnector> logger) : ApiBased(unitOfWork, settingsOptions), IAttendanceConnector
{
    private readonly IntegrationSettings _settings = settingsOptions.Value;

    public async Task<List<AttendanceRecord>> FetchAttendanceDataAsync()
    {
        logger.LogInformation("Fetching attendance data from Biotime from {Start} to {End}", _settings.Integration.StartDate, _settings.Integration.EndDate);

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
            var (records, nextURL) = await FetchPageAsync(token, page, startTime);

            if (records == null || records.Count == 0)
                break;

            allRecords.AddRange(records);

            if (string.IsNullOrEmpty(nextURL))
                break;

            page++;
        }

        logger.LogInformation("Total attendance records fetched: {TotalCount}", allRecords.Count);
        return allRecords;
    }

    private async Task<(List<AttendanceRecord>? records, string? nextURL)> FetchPageAsync(string token, int page, string startTime)
    {
        var response = await httpService.SendAsync<BioTimeResponseDTO>(new APIRequest
        {
            Url = $"{_settings.Integration.Server}/iclock/api/transactions/",
            Method = HttpMethod.Get,
            Token = token,
            QueryParameters = new Dictionary<string, string>
                {
                    { "page", page.ToString() },
                    { "page_size", _settings.Integration.PageSize },
                    { "start_time", startTime },
                    { "end_time", _settings.Integration.EndDate },
                },
            RequestContentType = HttpServiceContentTypes.application_json,
            ResponseContentType = HttpServiceContentTypes.application_json
        });

        if (!response.IsSuccess)
        {
            logger.LogError("Failed to fetch attendance data on page {Page}. Error: {Error}", page, response.ErrorMessage ?? "Unknown error");
            return (null, null);
        }

        if (response.Data?.BioTimePunches == null || response.Data.BioTimePunches.Count == 0)
        {
            logger.LogInformation("No attendance data found on page {Page}.", page);
            return ([], null);
        }

        var records = response.Data?.BioTimePunches?.ConvertAll(r => new AttendanceRecord
        {
            TId = r.ID.ToString() ?? string.Empty,
            EmployeeCode = r.EmployeeCode ?? string.Empty,
            PunchTime = r.PunchTime ?? string.Empty,
            Function = MapFunction(r.PunchStatus),
            MachineName = r.MachineName ?? string.Empty,
            MachineSerialNo = r.MachineSerialNo ?? string.Empty
        }) ?? [];

        return (records, response.Data.NextUrl);
    }

    private async Task<string?> GetTokenAsync()
    {
        var response = await httpService.SendAsync<TokenDTO>(new APIRequest
        {
            Method = HttpMethod.Post,
            Url = $"{_settings.Integration.Server}/jwt-api-token-auth/",
            Data = new
            {
                username = _settings.Integration.Username,
                password = _settings.Integration.Password
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