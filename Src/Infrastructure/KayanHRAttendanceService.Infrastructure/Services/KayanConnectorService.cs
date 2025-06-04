using KayanHRAttendanceService.Application.Interfaces;
using KayanHRAttendanceService.Application.Interfaces.Services;
using KayanHRAttendanceService.Domain.Entities.General;
using KayanHRAttendanceService.Domain.Entities.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KayanHRAttendanceService.Infrastructure.Services;

public class KayanConnectorService(IHttpService httpService, IOptions<IntegrationSettings> settingsOptions, ILogger<KayanConnectorService> logger) : IKayanConnectorService
{
    private readonly IntegrationSettings _settings = settingsOptions.Value;

    public async Task PushToKayanConnectorEndPoint(List<AttendanceRecord>? records)
    {
        var response = await httpService.SendAsync<object>(new Domain.Entities.Services.APIRequest
        {
            Url = _settings.APIBulkEndpoint,
            CustomHeaders = new Dictionary<string, string> { { "client_id", _settings.ClientID }, { "client_secret", _settings.ClientSecret } },
            TimeoutSeconds = 30,
            Method = HttpMethod.Post,
            RequestContentType = Domain.Entities.Services.HttpServiceContentTypes.application_json,
            ResponseContentType = Domain.Entities.Services.HttpServiceContentTypes.application_json,
            Data = new { }
        });
    }
}