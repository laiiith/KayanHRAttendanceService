using KayanHRAttendanceService.Application.Interfaces.Data;
using KayanHRAttendanceService.Domain.Entities.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace KayanHRAttendanceService.Infrastructure.Data.Persistence.Sqlite;

public class AttendanceDataRepository(ApplicationDbContext.ApplicationDbContext dbContext) : Repository<AttendanceRecord>(dbContext), IAttendanceDataRepository
{
    private readonly ApplicationDbContext.ApplicationDbContext _dbContext = dbContext;

    public async Task<string> GetLastPunchTime()
    {
        var dbSet = _dbContext.Set<AttendanceRecord>();
        return await dbSet.Where(x => !string.IsNullOrEmpty(x.PunchTime)).MaxAsync(x => x.PunchTime);
    }
}