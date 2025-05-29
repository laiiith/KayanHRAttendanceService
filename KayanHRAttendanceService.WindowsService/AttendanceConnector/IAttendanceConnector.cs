namespace KayanHRAttendanceService.WindowsService.AttendanceConnector;

public interface IAttendanceConnector
{
    Task<List<KayanHRAttendanceService.WindowsService.Storage.Models.AttendanceRecord>> FetchAttendanceAsync();
}
