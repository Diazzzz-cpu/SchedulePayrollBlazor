using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchedulePayrollBlazor.Migrations
{
    /// <inheritdoc />
    public partial class AddAttendancePenaltySettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_auto_generated",
                table: "payroll_adjustments",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "source",
                table: "payroll_adjustments",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "attendance_penalty_settings",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    late_penalty_per_minute = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    undertime_penalty_per_minute = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    absence_full_day_multiplier = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    overtime_bonus_per_minute = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_attendance_penalty_settings", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "attendance_penalty_settings");

            migrationBuilder.DropColumn(
                name: "is_auto_generated",
                table: "payroll_adjustments");

            migrationBuilder.DropColumn(
                name: "source",
                table: "payroll_adjustments");
        }
    }
}
