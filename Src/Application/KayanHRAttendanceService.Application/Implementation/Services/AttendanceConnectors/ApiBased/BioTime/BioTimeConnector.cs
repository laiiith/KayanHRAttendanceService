using KayanHRAttendanceService.Application.DTO;
using KayanHRAttendanceService.Application.Interfaces;
using KayanHRAttendanceService.Application.Interfaces.Services.AttendanceConnectors;
using KayanHRAttendanceService.Domain.Entities.General;
using KayanHRAttendanceService.Domain.Entities.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KayanHRAttendanceService.Application.Implementation.Services.AttendanceConnectors.ApiBased.BioTime;

public class BioTimeConnector(IHttpService httpService, IOptions<IntegrationSettings> settings, ILogger<BioTimeConnector> logger) : IAttendanceConnector
{
    public async Task<List<AttendanceRecord>> FetchAttendanceDataAsync()
    {
        logger.LogInformation("Fetching attendance data from BioTime from {Start} to {End}", settings.Value.StartDate, settings.Value.EndDate);
        var punches = new List<AttendanceRecord>();
        var token = await AuthenticateAsync();

        int page = 1;
        while (true)
        {
            string url = $"{settings.Value.Server}/iclock/api/transactions/?page={page}&page_size={settings.Value.PageSize}&start_time={settings.Value.StartDate}&end_time={settings.Value.EndDate}";
            logger.LogDebug("Requesting page {Page} from BioTime API", page);

            var response = await httpService.SendAsync<List<BioTimeResponseDTO>>(new Domain.Entities.Services.APIRequest
            {
                Method = HttpMethod.Get,
                Token = token,
                Url = url
            });

            if (response == null || response.Count == 0)
            {
                logger.LogInformation("No more data found. Stopping at page {Page}", page);
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

        logger.LogInformation("Fetched {Count} attendance records", punches.Count);
        return punches;
    }

    public async Task<string> AuthenticateAsync()
    {
        var response = await httpService.SendAsync<TokenDTO>(new Domain.Entities.Services.APIRequest
        {
            Method = HttpMethod.Post,
            Url = $"{settings.Value.Server}/jwt-api-token-auth/",
            Data = new { username = settings.Value.Username, password = settings.Value.Password }
        });

        if (response == null || string.IsNullOrEmpty(response.AccessToken))
        {
            logger.LogError("Authentication failed: token is null or empty");
            throw new Exception("Authentication failed: token response is null or empty");
        }

        return response.AccessToken;
    }
}
