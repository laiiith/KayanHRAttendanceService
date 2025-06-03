namespace KayanHRAttendanceService.Domain.Entities.General;

public class IntegrationSettings
{
    public int Type { get; set; }
    public string? Server { get; set; }
    public string ConnectionString { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string PageSize { get; set; }
    public string StartDate { get; set; }
    public string EndDate { get; set; }
    public string UpdateProcedure { get; set; }
    public string GetDataProcedure { get; set; }
    public bool DynamicDate { get; set; }
    public FunctionMapping Function_Mapping { get; set; }
}

public class FunctionMapping
{
    public string Attendance_In { get; set; }
    public string Attendance_Out { get; set; }
    public string Break_In { get; set; }
    public string Break_Out { get; set; }
    public string Permission_In { get; set; }
    public string Permission_Out { get; set; }
    public string Overtime_In { get; set; }
    public string Overtime_Out { get; set; }
}