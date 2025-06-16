using KayanHRAttendanceService.Domain.Entities.General;
using Microsoft.Extensions.Options;

namespace KayanHRAttendanceService.Application.Implementation.Services.AttendanceConnectors;

public class AttendanceConnectors(IOptions<IntegrationSettings> settingsOptions)
{
    private readonly IntegrationSettings _settings = settingsOptions.Value;

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