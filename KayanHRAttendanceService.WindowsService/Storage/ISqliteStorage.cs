namespace KayanHRAttendanceService.WindowsService.Storage;

public interface ISqliteStorage
{
    Task<List<KayanHRAttendanceService.WindowsService.Storage.Models.AttendanceRecord>> GetDataAsync(CancellationToken cancellationToken = default);
    Task PushDataAsync(List<KayanHRAttendanceService.WindowsService.Storage.Models.AttendanceRecord> model, CancellationToken cancellationToken = default);
    Task InitializeDatabaseAsync(CancellationToken cancellationToken = default);
}
