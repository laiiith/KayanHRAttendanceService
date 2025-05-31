using Microsoft.EntityFrameworkCore;

namespace KayanHRAttendanceService.Infrastructure.ApplicationDbContext;
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<KayanHRAttendanceService.Entities.Sqlite.AttendanceRecord> AttendanceData { get; set; }
}
