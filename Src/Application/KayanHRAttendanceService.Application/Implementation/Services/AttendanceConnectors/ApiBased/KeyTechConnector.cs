using System.Text.Json;
using KayanHRAttendanceService.Application.Interfaces;
using KayanHRAttendanceService.Application.Interfaces.Data;
using KayanHRAttendanceService.Application.Interfaces.Services.AttendanceConnectors;
using KayanHRAttendanceService.Domain.Entities.General;
using KayanHRAttendanceService.Domain.Entities.Services;
using KayanHRAttendanceService.Domain.Entities.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KayanHRAttendanceService.Application.Implementation.Services.AttendanceConnectors.ApiBased;

public class KeyTechConnector(IHttpService httpService, IUnitOfWork unitOfWork, IOptions<IntegrationSettings> settingsOptions, ILogger<KeyTechConnector> logger) : ApiBased(unitOfWork, settingsOptions), IAttendanceConnector
{
    private readonly IHttpService _httpService = httpService;
    private readonly IntegrationSettings _settings = settingsOptions.Value;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<KeyTechConnector> _logger = logger;

    public async Task<List<AttendanceRecord>> FetchAttendanceDataAsync()
    {
        _logger.LogInformation("Fetching attendance data from KeyTech from {Start} to {End}",
        _settings.Integration.StartDate, _settings.Integration.EndDate);

        var startDate = await DetermineStartTimeAsync();

        var requestBody = new
        {
            entityId = Convert.ToInt32(_settings.Integration.EntityId),
            Fromdate = startDate,
            Todate = _settings.Integration.EndDate
        };

        var request = new APIRequest
        {
            Method = HttpMethod.Post,
            Url = "https://mobile.smsbykeytech.com/SMSAPI/GetAttendanceData",
            Data = requestBody,
            Token = _settings.Integration.Token,
            RequestContentType = HttpServiceContentTypes.application_json
        };

        var response = await _httpService.SendAsync<KeyTechApiResponseDTO>(request);

        if (!response.IsSuccess || response.Data == null || response.Data.Result != "Succes")
        {
            _logger.LogWarning("KeyTech API returned failure or unexpected result: {Response}", JsonSerializer.Serialize(response));
            return [];
        }

        var records = new List<AttendanceRecord>();

        foreach (var item in response.Data.Data ?? [])
        {
            if (string.IsNullOrWhiteSpace(item.Timestamp))
                continue;

            records.Add(new AttendanceRecord
            {
                TId = item.Index.ToString(),
                EmployeeCode = item.EmployeeCode,
                PunchTime = item.Timestamp,
                Function = MapFunction(item.Type),
                MachineName = item.Machine
            });

            _logger.LogInformation("Fetched punch from KeyTech at {Time}", item.Timestamp);
        }

        return records;
    }
}
