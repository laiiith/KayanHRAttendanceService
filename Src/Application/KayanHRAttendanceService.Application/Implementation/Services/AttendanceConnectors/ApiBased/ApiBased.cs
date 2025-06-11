using KayanHRAttendanceService.Application.Interfaces.Data;
using KayanHRAttendanceService.Domain.Entities.General;
using Microsoft.Extensions.Options;

namespace KayanHRAttendanceService.Application.Implementation.Services.AttendanceConnectors.ApiBased;

public class ApiBased(IUnitOfWork unitOfWork, IOptions<IntegrationSettings> settingsOptions) : AttendanceConnectors(settingsOptions)
{
    private readonly IntegrationSettings _settings = settingsOptions.Value;

    protected async Task<string> DetermineStartTimeAsync()
    {
        if (!_settings.DynamicDate)
            return _settings.Integration.StartDate!;

        var lastPunchTime = await unitOfWork.AttendanceData.GetLastPunchTime();
        return !string.IsNullOrWhiteSpace(lastPunchTime) ? lastPunchTime : _settings.Integration.StartDate!;
    }
}