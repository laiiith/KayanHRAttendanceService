namespace KayanHRAttendanceService.Domain.Entities.General;

public class IntegrationSettings
{
    public required int Type { get; set; }
    public required string Interval { get; set; }
    public required string APIBulkEndpoint { get; set; }
    public required string ClientKey { get; set; }
    public required string ClientSecret { get; set; }
    public required string BatchSize { get; set; }
    public required bool Debug { get; set; }
    public required bool DynamicDate { get; set; }
    public required FunctionMapping FunctionMapping { get; set; }
    public required Integration Integration { get; set; }
}

public class FunctionMapping
{
    public required string AttendanceIn { get; set; }
    public required string AttendanceOut { get; set; }
    public required string BreakIn { get; set; }
    public required string BreakOut { get; set; }
    public required string PermissionIn { get; set; }
    public required string PermissionOut { get; set; }
    public required string OvertimeIn { get; set; }
    public required string OvertimeOut { get; set; }
}

public class Integration
{
    public string? Server { get; set; }
    public string? ConnectionString { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public required string PageSize { get; set; }
    public required string StartDate { get; set; }
    public required string EndDate { get; set; }
    public string? UpdateDataProcedure { get; set; }
    public string? FetchDataProcedure { get; set; }
}