using Microsoft.EntityFrameworkCore;

namespace KayanHRAttendanceService.Infrastructure.ApplicationDbContext;
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<KayanHRAttendanceService.Domain.Entities.Sqlite.AttendanceRecord> AttendanceData { get; set; }
}
