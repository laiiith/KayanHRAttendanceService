using KayanHRAttendanceService.Domain.Entities.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace KayanHRAttendanceService.Infrastructure.Data.ApplicationDbContext;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Domain.Entities.Sqlite.AttendanceRecord> AttendanceData { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AttendanceRecord>()
            .HasIndex(a => new { a.EmployeeCode, a.PunchTime })
            .IsUnique();
    }
}