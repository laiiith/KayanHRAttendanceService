using KayanHRAttendanceService.Entities.Sqlite;

namespace KayanHRAttendanceService.Storage.SqliteDataAccess;

public interface ISqliteStorage
{
    Task<List<AttendanceRecord>> GetDataAsync(CancellationToken cancellationToken = default);
    Task PushDataAsync(List<AttendanceRecord> model, CancellationToken cancellationToken = default);
    Task InitializeDatabaseAsync(CancellationToken cancellationToken = default);
}
