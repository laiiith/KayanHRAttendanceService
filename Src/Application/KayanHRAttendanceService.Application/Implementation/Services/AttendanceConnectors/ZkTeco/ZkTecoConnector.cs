using KayanHRAttendanceService.Application.Interfaces.Services.AttendanceConnectors;
using KayanHRAttendanceService.Domain.Entities.General;
using KayanHRAttendanceService.Domain.Entities.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KayanHRAttendanceService.Application.Implementation.Services.AttendanceConnectors.ZkTeco;

public class ZkTecoConnector(IOptions<IntegrationSettings> settingsOptions, ILogger<ZkTecoConnector> logger) : AttendanceConnectors(settingsOptions), IAttendanceConnector
{
    private readonly IntegrationSettings _settings = settingsOptions.Value;
    private readonly ILogger<ZkTecoConnector> _logger = logger;
    public Task<List<AttendanceRecord>> FetchAttendanceDataAsync()
    {
        throw new NotImplementedException();
    }
}
