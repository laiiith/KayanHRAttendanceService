namespace KayanHRAttendanceService.WindowsService.Storage;

public interface ISqliteStorage
{
    Task<List<(int Id, DateTime Time)>> GetDataAsync(CancellationToken cancellationToken = default);
    Task PushDataAsync(string message, DateTime createdAt, CancellationToken cancellationToken = default);
    Task InitializeDatabaseAsync(CancellationToken cancellationToken = default);
}
