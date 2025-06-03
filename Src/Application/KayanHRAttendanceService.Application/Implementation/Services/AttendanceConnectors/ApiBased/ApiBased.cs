using KayanHRAttendanceService.Application.Interfaces.Data;
using KayanHRAttendanceService.Domain.Entities.General;
using Microsoft.Extensions.Options;

namespace KayanHRAttendanceService.Application.Implementation.Services.AttendanceConnectors.ApiBased;

public class ApiBased(IUnitOfWork unitOfWork, IOptions<IntegrationSettings> settingsOptions)
{
    private readonly IntegrationSettings _settings = settingsOptions.Value;

    protected async Task<string> DetermineStartTimeAsync()
    {
        if (!_settings.DynamicDate)
            return _settings.Integration.StartDate;

        var lastPunchTime = await unitOfWork.AttendanceData.GetLastPunchTime();
        return !string.IsNullOrWhiteSpace(lastPunchTime) ? lastPunchTime : _settings.Integration.StartDate;
    }

    protected string MapFunction(string? function)
    {
        if (function == null)
            return "attendance-in";

        if (function == _settings.FunctionMapping.AttendanceIn)
            return "attendance-in";
        if (function == _settings.FunctionMapping.AttendanceOut)
            return "attendance-out";
        if (function == _settings.FunctionMapping.BreakIn)
            return "break-in";
        if (function == _settings.FunctionMapping.BreakOut)
            return "break-out";
        if (function == _settings.FunctionMapping.PermissionIn)
            return "permission-in";
        if (function == _settings.FunctionMapping.PermissionOut)
            return "permission-out";
        if (function == _settings.FunctionMapping.OvertimeIn)
            return "overtime-in";
        if (function == _settings.FunctionMapping.OvertimeOut)
            return "overtime-out";

        return "attendance-in";
    }
}