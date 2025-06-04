using KayanHRAttendanceService.Domain.Entities.Sqlite;

namespace KayanHRAttendanceService.Application.Interfaces.Data;

public interface IAttendanceDataRepository : IRepository<AttendanceRecord>
{
    Task<string> GetLastPunchTime();
    Task UpdateAsync(IEnumerable<AttendanceRecord> records);
}