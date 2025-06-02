using Microsoft.EntityFrameworkCore;

namespace KayanHRAttendanceService.Infrastructure.Data.ApplicationDbContext;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Domain.Entities.Sqlite.AttendanceRecord> AttendanceData { get; set; }
}