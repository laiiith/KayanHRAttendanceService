namespace KayanHRAttendanceService.Application.DTO;

public class KayanConnectorAttendanceDTO
{
    public string tid { get; set; }
    public string EmployeeCardNumber { get; set; }
    public DateTime AttendanceDate { get; set; }
    public string FunctionType { get; set; }
    public string MachineName { get; set; }
}
