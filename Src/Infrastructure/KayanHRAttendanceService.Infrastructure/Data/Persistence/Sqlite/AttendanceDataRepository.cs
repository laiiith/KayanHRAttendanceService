using KayanHRAttendanceService.Domain.Entities.Sqlite;
using KayanHRAttendanceService.Domain.Interfaces;

namespace KayanHRAttendanceService.Infrastructure.Data.Persistence.Sqlite;

public class AttendanceDataRepository(ApplicationDbContext.ApplicationDbContext dbContext) : Repository<AttendanceRecord>(dbContext), IAttendanceDataRepository
{
}
