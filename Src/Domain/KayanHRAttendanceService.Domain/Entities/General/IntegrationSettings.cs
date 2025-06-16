namespace KayanHRAttendanceService.Domain.Entities.General;

public class IntegrationSettings
{
    public int Type { get; set; }
    public int Interval { get; set; }
    public string APIBulkEndpoint { get; set; }
    public string ClientID { get; set; }
    public string ClientSecret { get; set; }
    public int BatchSize { get; set; }
    public bool DynamicDate { get; set; }
    public FunctionMapping FunctionMapping { get; set; }
    public Integration Integration { get; set; }
    public ZkTecoSettings ZkTecoSettings { get; set; }
}

public class FunctionMapping
{
    public string AttendanceIn { get; set; }
    public string AttendanceOut { get; set; }
    public string BreakIn { get; set; }
    public string BreakOut { get; set; }
    public string PermissionIn { get; set; }
    public string PermissionOut { get; set; }
    public string OvertimeIn { get; set; }
    public string OvertimeOut { get; set; }
}

public class Integration
{
    public string? Server { get; set; }
    public string? ConnectionString { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? PageSize { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public string? UpdateDataProcedure { get; set; }
    public string? FetchDataProcedure { get; set; }
}

public class ZkTecoSettings
{
    public string? Host { get; set; }
    public string? Port { get; set; }
}