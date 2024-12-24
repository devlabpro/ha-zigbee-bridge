using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZigbeeBridgeAddon.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAreaAndName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Area",
                table: "Devices",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Devices",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Area",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Devices");
        }
    }
}
