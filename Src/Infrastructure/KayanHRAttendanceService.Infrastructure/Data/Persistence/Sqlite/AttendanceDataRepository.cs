using KayanHRAttendanceService.Application.Interfaces.Data;
using KayanHRAttendanceService.Domain.Entities.Sqlite;

namespace KayanHRAttendanceService.Infrastructure.Data.Persistence.Sqlite;

public class AttendanceDataRepository(ApplicationDbContext.ApplicationDbContext dbContext) : Repository<AttendanceRecord>(dbContext), IAttendanceDataRepository
{
}
