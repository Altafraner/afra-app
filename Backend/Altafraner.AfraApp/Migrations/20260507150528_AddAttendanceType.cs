using Altafraner.AfraApp.Attendance.Domain.Models;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altafraner.AfraApp.Migrations
{
    /// <inheritdoc />
    public partial class AddAttendanceType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<AttendanceEntryType>(
                name: "entry_type",
                table: "attendances",
                type: "attendance_entry_type",
                nullable: false,
                defaultValue: AttendanceEntryType.Manual);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "entry_type",
                table: "attendances");
        }
    }
}
