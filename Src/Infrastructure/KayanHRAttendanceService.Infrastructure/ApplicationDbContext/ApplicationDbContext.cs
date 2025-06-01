using Microsoft.EntityFrameworkCore;

namespace KayanHRAttendanceService.Infrastructure.ApplicationDbContext;
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Domain.Entities.Sqlite.AttendanceRecord> AttendanceData { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Domain.Entities.Sqlite.AttendanceRecord>().HasData(
               new Domain.Entities.Sqlite.AttendanceRecord
               {
                   ID = 1,
                   EmployeeCode = "EMP001",
                   PunchTime = "2025-06-01 08:00:00",
                   Function = "IN",
                   MachineName = "MainGate",
                   MachineSerialNo = "SN001",
                   Status = "Verified",
                   TId = "T1001"
               },
               new Domain.Entities.Sqlite.AttendanceRecord
               {
                   ID = 2,
                   EmployeeCode = "EMP002",
                   PunchTime = "2025-06-01 08:15:00",
                   Function = "IN",
                   MachineName = "SideGate",
                   MachineSerialNo = "SN002",
                   Status = "Verified",
                   TId = "T1002"
               },
               new Domain.Entities.Sqlite.AttendanceRecord
               {
                   ID = 3,
                   EmployeeCode = "EMP001",
                   PunchTime = "2025-06-01 17:00:00",
                   Function = "OUT",
                   MachineName = "MainGate",
                   MachineSerialNo = "SN001",
                   Status = "Verified",
                   TId = "T1003"
               },
               new Domain.Entities.Sqlite.AttendanceRecord
               {
                   ID = 4,
                   EmployeeCode = "EMP002",
                   PunchTime = "2025-06-01 17:05:00",
                   Function = "OUT",
                   MachineName = "SideGate",
                   MachineSerialNo = "SN002",
                   Status = "Verified",
                   TId = "T1004"
               }
           );
    }
}
