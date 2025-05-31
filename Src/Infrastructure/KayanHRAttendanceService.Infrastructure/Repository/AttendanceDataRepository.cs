using KayanHRAttendanceService.Application.Common.Interfaces;
using KayanHRAttendanceService.Domain.Entities.Sqlite;

namespace KayanHRAttendanceService.Infrastructure.Repository;

public class AttendanceDataRepository(ApplicationDbContext.ApplicationDbContext dbContext) : Repository.Repository<AttendanceRecord>(dbContext), IAttendanceDataRepository
{
}
