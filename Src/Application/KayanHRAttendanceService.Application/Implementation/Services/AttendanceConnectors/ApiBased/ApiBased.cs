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
            return _settings.StartDate;

        var lastPunchTime = await unitOfWork.AttendanceData.GetLastPunchTime();
        return !string.IsNullOrWhiteSpace(lastPunchTime) ? lastPunchTime : _settings.StartDate;
    }

    protected string MapFunction(string? function)
    {
        if (function == null)
            return "attendance-in";

        if (function == _settings.Function_Mapping.Attendance_In)
            return "attendance-in";
        if (function == _settings.Function_Mapping.Attendance_Out)
            return "attendance-out";
        if (function == _settings.Function_Mapping.Break_In)
            return "break-in";
        if (function == _settings.Function_Mapping.Break_Out)
            return "break-out";
        if (function == _settings.Function_Mapping.Permission_In)
            return "permission-in";
        if (function == _settings.Function_Mapping.Permission_Out)
            return "permission-out";
        if (function == _settings.Function_Mapping.Overtime_In)
            return "overtime-in";
        if (function == _settings.Function_Mapping.Overtime_Out)
            return "overtime-out";

        return "attendance-in";
    }
}