using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchedulePayrollBlazor.Migrations
{
    /// <inheritdoc />
    public partial class AddPayrollModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "employee_compensation",
                columns: table => new
                {
                    employee_id = table.Column<int>(type: "int", nullable: false),
                    is_hourly = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    hourly_rate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    fixed_monthly_salary = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employee_compensation", x => x.employee_id);
                    table.ForeignKey(
                        name: "FK_employee_compensation_employees_employee_id",
                        column: x => x.employee_id,
                        principalTable: "employees",
                        principalColumn: "employee_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "payroll_periods",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    start_date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    end_date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payroll_periods", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "payroll_entries",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    payroll_period_id = table.Column<int>(type: "int", nullable: false),
                    employee_id = table.Column<int>(type: "int", nullable: false),
                    total_hours_worked = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    base_pay = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    total_deductions = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    total_bonuses = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    net_pay = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    calculated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payroll_entries", x => x.id);
                    table.ForeignKey(
                        name: "FK_payroll_entries_employees_employee_id",
                        column: x => x.employee_id,
                        principalTable: "employees",
                        principalColumn: "employee_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_payroll_entries_payroll_periods_payroll_period_id",
                        column: x => x.payroll_period_id,
                        principalTable: "payroll_periods",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "payroll_adjustments",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    payroll_entry_id = table.Column<int>(type: "int", nullable: false),
                    type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    label = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payroll_adjustments", x => x.id);
                    table.ForeignKey(
                        name: "FK_payroll_adjustments_payroll_entries_payroll_entry_id",
                        column: x => x.payroll_entry_id,
                        principalTable: "payroll_entries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_payroll_adjustments_payroll_entry_id",
                table: "payroll_adjustments",
                column: "payroll_entry_id");

            migrationBuilder.CreateIndex(
                name: "IX_payroll_entries_employee_id",
                table: "payroll_entries",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_payroll_entries_payroll_period_id",
                table: "payroll_entries",
                column: "payroll_period_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "employee_compensation");

            migrationBuilder.DropTable(
                name: "payroll_adjustments");

            migrationBuilder.DropTable(
                name: "payroll_entries");

            migrationBuilder.DropTable(
                name: "payroll_periods");
        }
    }
}
