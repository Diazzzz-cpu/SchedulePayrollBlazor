using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchedulePayrollBlazor.Migrations
{
    /// <inheritdoc />
    public partial class Fix_RolePayComponents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "group_name",
                table: "shifts",
                type: "varchar(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(150)",
                oldMaxLength: 150)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "pay_components",
                keyColumn: "calculation_type",
                keyValue: null,
                column: "calculation_type",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "calculation_type",
                table: "pay_components",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "role_pay_components",
                columns: table => new
                {
                    role_pay_component_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    role_id = table.Column<int>(type: "int", nullable: false),
                    pay_component_id = table.Column<int>(type: "int", nullable: false),
                    default_rate_override = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_pay_components", x => x.role_pay_component_id);
                    table.ForeignKey(
                        name: "FK_role_pay_components_pay_components_pay_component_id",
                        column: x => x.pay_component_id,
                        principalTable: "pay_components",
                        principalColumn: "pay_component_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_role_pay_components_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "role_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_role_pay_components_pay_component_id",
                table: "role_pay_components",
                column: "pay_component_id");

            migrationBuilder.CreateIndex(
                name: "IX_role_pay_components_role_id",
                table: "role_pay_components",
                column: "role_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "role_pay_components");

            migrationBuilder.UpdateData(
                table: "shifts",
                keyColumn: "group_name",
                keyValue: null,
                column: "group_name",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "group_name",
                table: "shifts",
                type: "varchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(150)",
                oldMaxLength: 150,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "calculation_type",
                table: "pay_components",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
