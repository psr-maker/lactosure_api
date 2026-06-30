using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lactosure_api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMachineFieldsFromCorrMethodHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MId",
                table: "CorrMethodHistory");

            migrationBuilder.DropColumn(
                name: "Model",
                table: "CorrMethodHistory");

            migrationBuilder.DropColumn(
                name: "SId",
                table: "CorrMethodHistory");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MId",
                table: "CorrMethodHistory",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "CorrMethodHistory",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "SId",
                table: "CorrMethodHistory",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
