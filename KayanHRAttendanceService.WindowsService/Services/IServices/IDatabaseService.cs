namespace KayanHRAttendanceService.WindowsService.Services.IServices;

public interface IDatabaseService
{
    Task<List<(int Id, DateTime Time)>> GetDataAsync(CancellationToken cancellationToken = default);
    Task PushDataAsync(string message, DateTime createdAt, CancellationToken cancellationToken = default);
    Task InitializeDatabaseAsync(CancellationToken cancellationToken = default);
}


