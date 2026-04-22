using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salon_LeHoang.Migrations
{
    /// <inheritdoc />
    public partial class InitialCodeFirst : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ServiceCode",
                table: "Services",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LateDays",
                table: "Attendances",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "LateNotes",
                table: "Attendances",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ServiceCode",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "LateDays",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "LateNotes",
                table: "Attendances");
        }
    }
}
