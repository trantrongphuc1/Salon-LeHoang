using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salon_LeHoang.Migrations
{
    /// <inheritdoc />
    public partial class DynamicCategoryCommissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CommissionRate1",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "CommissionRate2",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "CommissionRate3",
                table: "Employees");

            migrationBuilder.CreateTable(
                name: "EmployeeCommissions",
                columns: table => new
                {
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    CommissionRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false, defaultValue: 0m)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeCommissions", x => new { x.EmployeeId, x.CategoryId });
                    table.ForeignKey(
                        name: "FK_EmployeeCommissions_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeCommissions_ServiceCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ServiceCategories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeCommissions_CategoryId",
                table: "EmployeeCommissions",
                column: "CategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeCommissions");

            migrationBuilder.AddColumn<decimal>(
                name: "CommissionRate1",
                table: "Employees",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CommissionRate2",
                table: "Employees",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CommissionRate3",
                table: "Employees",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
