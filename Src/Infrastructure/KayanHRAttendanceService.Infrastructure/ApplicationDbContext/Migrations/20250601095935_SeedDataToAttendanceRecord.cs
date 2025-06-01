using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace KayanHRAttendanceService.Infrastructure.ApplicationDbContext.Migrations
{
    /// <inheritdoc />
    public partial class SeedDataToAttendanceRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AttendanceData",
                columns: new[] { "ID", "EmployeeCode", "Function", "MachineName", "MachineSerialNo", "PunchTime", "Status", "TId" },
                values: new object[,]
                {
                    { 1, "EMP001", "IN", "MainGate", "SN001", "2025-06-01 08:00:00", "Verified", "T1001" },
                    { 2, "EMP002", "IN", "SideGate", "SN002", "2025-06-01 08:15:00", "Verified", "T1002" },
                    { 3, "EMP001", "OUT", "MainGate", "SN001", "2025-06-01 17:00:00", "Verified", "T1003" },
                    { 4, "EMP002", "OUT", "SideGate", "SN002", "2025-06-01 17:05:00", "Verified", "T1004" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AttendanceData",
                keyColumn: "ID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "AttendanceData",
                keyColumn: "ID",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "AttendanceData",
                keyColumn: "ID",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "AttendanceData",
                keyColumn: "ID",
                keyValue: 4);
        }
    }
}
