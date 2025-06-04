using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KayanHRAttendanceService.Infrastructure.Data.ApplicationDbContext.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueConstraintToAttendance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AttendanceData_EmployeeCode_PunchTime",
                table: "AttendanceData",
                columns: new[] { "EmployeeCode", "PunchTime" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AttendanceData_EmployeeCode_PunchTime",
                table: "AttendanceData");
        }
    }
}
