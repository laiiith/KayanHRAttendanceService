using KayanHRAttendanceService.Application.DTO;
using KayanHRAttendanceService.Application.Interfaces;
using KayanHRAttendanceService.Application.Interfaces.Services;
using KayanHRAttendanceService.Domain.Entities.General;
using KayanHRAttendanceService.Domain.Entities.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KayanHRAttendanceService.Infrastructure.Services;

public class KayanConnectorService(IHttpService httpService, IOptions<IntegrationSettings> settingsOptions, ILogger<KayanConnectorService> logger) : IKayanConnectorService
{
    private readonly ILogger<KayanConnectorService> _logger = logger;

    private readonly IHttpService _httpService = httpService;

    private readonly IntegrationSettings _settings = settingsOptions.Value;

    public async Task<(bool IsSuccess, int StatusID)> PushToKayanConnectorEndPoint(List<AttendanceRecord> records)
    {
        var response = await _httpService.SendAsync<KayanConnectorResponseDTO>(new Domain.Entities.Services.APIRequest
        {
            Url = _settings.APIBulkEndpoint,
            CustomHeaders = new Dictionary<string, string> { { "clientID", _settings.ClientID }, { "ClientSecret", _settings.ClientSecret } },
            TimeoutSeconds = 30,
            Method = HttpMethod.Post,
            RequestContentType = Domain.Entities.Services.HttpServiceContentTypes.application_json,
            ResponseContentType = Domain.Entities.Services.HttpServiceContentTypes.application_json,
            Data = records.Select(x => new KayanConnectorAttendanceDTO
            {
                tid = x.TId,
                EmployeeCardNumber = x.EmployeeCode,
                AttendanceDate = DateTime.TryParse(x.PunchTime, out var parsedDate) ? parsedDate : default,
                FunctionType = x.Function,
                MachineName = x.MachineName
            }).ToList()
        });

        if (!response.IsSuccess || response.StatusCode != 200)
        {
            _logger.LogError("Failed to push records: HTTP {StatusCode} - {Error}", response.StatusCode, response.ErrorMessage);
        }

        return (response.IsSuccess, response.StatusCode);
    }
}