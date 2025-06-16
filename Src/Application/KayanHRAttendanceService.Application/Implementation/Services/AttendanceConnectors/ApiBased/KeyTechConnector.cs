using KayanHRAttendanceService.Application.Interfaces;
using KayanHRAttendanceService.Application.Interfaces.Data;
using KayanHRAttendanceService.Application.Interfaces.Services.AttendanceConnectors;
using KayanHRAttendanceService.Domain.Entities.General;
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

    public Task<List<AttendanceRecord>> FetchAttendanceDataAsync()
    {
        logger.LogInformation("Fetching attendance data from KeyTech from {Start} to {End}", _settings.Integration.StartDate, _settings.Integration.EndDate);

        throw new NotImplementedException();
    }
}