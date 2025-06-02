namespace KayanHRAttendanceService.Domain.Entities.General;

public class IntegrationSettings
{
    public int Type { get; set; }
    public string? Server { get; set; }
    public string ConnectionString { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? PageSize { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public bool DynamicDate { get; set; }
    public string UpdateProcedure { get; set; }
    public string GetDataProcedure { get; set; }
}