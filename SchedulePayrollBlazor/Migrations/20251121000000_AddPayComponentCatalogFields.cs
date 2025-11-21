using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchedulePayrollBlazor.Migrations
{
    /// <inheritdoc />
    public partial class AddPayComponentCatalogFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "calculation_type",
                table: "pay_components",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "pay_components",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "calculation_type",
                table: "pay_components");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "pay_components");
        }
    }
}
