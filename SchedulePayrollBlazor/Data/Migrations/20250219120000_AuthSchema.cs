using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchedulePayrollBlazor.Data.Migrations;

public partial class AuthSchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                Email = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                PasswordHash = table.Column<string>(type: "longtext", nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Role = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Users", x => x.Id);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "PayPeriods",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                PeriodName = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                StartDate = table.Column<DateTime>(type: "date", nullable: false),
                EndDate = table.Column<DateTime>(type: "date", nullable: false),
                Status = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false, defaultValue: "Open")
                    .Annotation("MySql:CharSet", "utf8mb4")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PayPeriods", x => x.Id);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "Employees",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                UserId = table.Column<int>(type: "int", nullable: false),
                FirstName = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                LastName = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Department = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                JobTitle = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                EmploymentType = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false, defaultValue: "FullTime")
                    .Annotation("MySql:CharSet", "utf8mb4"),
                StartDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                Location = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Employees", x => x.Id);
                table.ForeignKey(
                    name: "FK_Employees_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "Schedules",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                EmployeeId = table.Column<int>(type: "int", nullable: false),
                ShiftDate = table.Column<DateTime>(type: "date", nullable: false),
                StartTime = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                EndTime = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                Source = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false, defaultValue: "Manual")
                    .Annotation("MySql:CharSet", "utf8mb4")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Schedules", x => x.Id);
                table.ForeignKey(
                    name: "FK_Schedules_Employees_EmployeeId",
                    column: x => x.EmployeeId,
                    principalTable: "Employees",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "PayrollRuns",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                EmployeeId = table.Column<int>(type: "int", nullable: false),
                PayPeriodId = table.Column<int>(type: "int", nullable: true),
                Status = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false, defaultValue: "Pending")
                    .Annotation("MySql:CharSet", "utf8mb4"),
                GrossPay = table.Column<decimal>(type: "decimal(12,2)", nullable: false, defaultValue: 0m),
                TotalDeductions = table.Column<decimal>(type: "decimal(12,2)", nullable: false, defaultValue: 0m),
                NetPay = table.Column<decimal>(type: "decimal(12,2)", nullable: false, defaultValue: 0m),
                CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PayrollRuns", x => x.Id);
                table.ForeignKey(
                    name: "FK_PayrollRuns_Employees_EmployeeId",
                    column: x => x.EmployeeId,
                    principalTable: "Employees",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_PayrollRuns_PayPeriods_PayPeriodId",
                    column: x => x.PayPeriodId,
                    principalTable: "PayPeriods",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateIndex(
            name: "IX_Employees_UserId",
            table: "Employees",
            column: "UserId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_PayrollRuns_EmployeeId",
            table: "PayrollRuns",
            column: "EmployeeId");

        migrationBuilder.CreateIndex(
            name: "IX_PayrollRuns_PayPeriodId",
            table: "PayrollRuns",
            column: "PayPeriodId");

        migrationBuilder.CreateIndex(
            name: "IX_Schedules_EmployeeId",
            table: "Schedules",
            column: "EmployeeId");

        migrationBuilder.CreateIndex(
            name: "IX_Users_Email",
            table: "Users",
            column: "Email",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "PayrollRuns");

        migrationBuilder.DropTable(
            name: "Schedules");

        migrationBuilder.DropTable(
            name: "PayPeriods");

        migrationBuilder.DropTable(
            name: "Employees");

        migrationBuilder.DropTable(
            name: "Users");
    }
}
