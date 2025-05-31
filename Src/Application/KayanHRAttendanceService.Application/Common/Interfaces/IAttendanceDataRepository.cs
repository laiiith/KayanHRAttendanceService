using KayanHRAttendanceService.Domain.Entities.Sqlite;

namespace KayanHRAttendanceService.Application.Common.Interfaces;

public interface IAttendanceDataRepository : IRepository<AttendanceRecord>
{
}
