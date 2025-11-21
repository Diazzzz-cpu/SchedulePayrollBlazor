using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchedulePayrollBlazor.Migrations
{
    /// <inheritdoc />
    public partial class AddPayrollLineBreakdown : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_payroll_lines_pay_components_pay_component_id",
                table: "payroll_lines");

            migrationBuilder.DropForeignKey(
                name: "FK_payroll_lines_payroll_runs_payroll_run_id",
                table: "payroll_lines");

            migrationBuilder.AlterColumn<int>(
                name: "payroll_run_id",
                table: "payroll_lines",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "pay_component_id",
                table: "payroll_lines",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "code",
                table: "payroll_lines",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "payroll_lines",
                type: "varchar(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "is_auto_generated",
                table: "payroll_lines",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "kind",
                table: "payroll_lines",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "payroll_entry_id",
                table: "payroll_lines",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_payroll_lines_payroll_entry_id",
                table: "payroll_lines",
                column: "payroll_entry_id");

            migrationBuilder.AddForeignKey(
                name: "FK_payroll_lines_payroll_entries_payroll_entry_id",
                table: "payroll_lines",
                column: "payroll_entry_id",
                principalTable: "payroll_entries",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_payroll_lines_pay_components_pay_component_id",
                table: "payroll_lines",
                column: "pay_component_id",
                principalTable: "pay_components",
                principalColumn: "pay_component_id");

            migrationBuilder.AddForeignKey(
                name: "FK_payroll_lines_payroll_runs_payroll_run_id",
                table: "payroll_lines",
                column: "payroll_run_id",
                principalTable: "payroll_runs",
                principalColumn: "payroll_run_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_payroll_lines_pay_components_pay_component_id",
                table: "payroll_lines");

            migrationBuilder.DropForeignKey(
                name: "FK_payroll_lines_payroll_entries_payroll_entry_id",
                table: "payroll_lines");

            migrationBuilder.DropForeignKey(
                name: "FK_payroll_lines_payroll_runs_payroll_run_id",
                table: "payroll_lines");

            migrationBuilder.DropIndex(
                name: "IX_payroll_lines_payroll_entry_id",
                table: "payroll_lines");

            migrationBuilder.DropColumn(
                name: "code",
                table: "payroll_lines");

            migrationBuilder.DropColumn(
                name: "description",
                table: "payroll_lines");

            migrationBuilder.DropColumn(
                name: "is_auto_generated",
                table: "payroll_lines");

            migrationBuilder.DropColumn(
                name: "kind",
                table: "payroll_lines");

            migrationBuilder.DropColumn(
                name: "payroll_entry_id",
                table: "payroll_lines");

            migrationBuilder.AlterColumn<int>(
                name: "payroll_run_id",
                table: "payroll_lines",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "pay_component_id",
                table: "payroll_lines",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_payroll_lines_pay_components_pay_component_id",
                table: "payroll_lines",
                column: "pay_component_id",
                principalTable: "pay_components",
                principalColumn: "pay_component_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_payroll_lines_payroll_runs_payroll_run_id",
                table: "payroll_lines",
                column: "payroll_run_id",
                principalTable: "payroll_runs",
                principalColumn: "payroll_run_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
