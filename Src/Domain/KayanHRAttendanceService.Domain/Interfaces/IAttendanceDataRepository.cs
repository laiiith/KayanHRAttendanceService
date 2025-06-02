using KayanHRAttendanceService.Domain.Entities.Sqlite;

namespace KayanHRAttendanceService.Domain.Interfaces;

public interface IAttendanceDataRepository : IRepository<AttendanceRecord>
{
}
