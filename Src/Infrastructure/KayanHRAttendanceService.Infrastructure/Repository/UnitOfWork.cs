using KayanHRAttendanceService.Application.Common.Interfaces;

namespace KayanHRAttendanceService.Infrastructure.Repository;

public class UnitOfWork(ApplicationDbContext.ApplicationDbContext dbContext) : IUnitOfWork
{
    public IAttendanceDataRepository AttendanceData => new AttendanceDataRepository(dbContext);

    public async Task Save() => await dbContext.SaveChangesAsync();
}
