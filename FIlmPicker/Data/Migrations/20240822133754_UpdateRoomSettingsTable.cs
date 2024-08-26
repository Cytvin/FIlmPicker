using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FIlmPicker.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRoomSettingsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "MinKpRating",
                table: "RoomSettings",
                type: "nvarchar(4)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "MaxKpRating",
                table: "RoomSettings",
                type: "nvarchar(4)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "MinKpRating",
                table: "RoomSettings",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(4)");

            migrationBuilder.AlterColumn<string>(
                name: "MaxKpRating",
                table: "RoomSettings",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(4)");
        }
    }
}
