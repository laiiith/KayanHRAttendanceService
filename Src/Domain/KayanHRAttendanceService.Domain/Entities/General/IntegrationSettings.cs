namespace KayanHRAttendanceService.Domain.Entities.General;

public class IntegrationSettings
{
    public int Type { get; set; }
    public string? Server { get; set; }
    public required string ConnectionString { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? Page_Size { get; set; }
    public string? Start_Date { get; set; }
    public string? End_Date { get; set; }
    public string? UpdateProcedure { get; set; }
    public required string GetDataProcedure { get; set; }
}