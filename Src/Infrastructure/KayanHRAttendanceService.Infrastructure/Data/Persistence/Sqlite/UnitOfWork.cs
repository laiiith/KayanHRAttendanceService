using KayanHRAttendanceService.Application.Interfaces.Data;

namespace KayanHRAttendanceService.Infrastructure.Data.Persistence.Sqlite;

public class UnitOfWork(ApplicationDbContext.ApplicationDbContext dbContext) : IUnitOfWork
{
    public IAttendanceDataRepository AttendanceData => new AttendanceDataRepository(dbContext);

    public async Task SaveChangesAsync() => await dbContext.SaveChangesAsync();
}