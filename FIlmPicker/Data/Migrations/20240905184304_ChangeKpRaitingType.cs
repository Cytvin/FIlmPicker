using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FIlmPicker.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeKpRaitingType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<float>(
                name: "MinKpRating",
                table: "RoomSettings",
                type: "real",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(4)");

            migrationBuilder.AlterColumn<float>(
                name: "MaxKpRating",
                table: "RoomSettings",
                type: "real",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(4)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "MinKpRating",
                table: "RoomSettings",
                type: "nvarchar(4)",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<string>(
                name: "MaxKpRating",
                table: "RoomSettings",
                type: "nvarchar(4)",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");
        }
    }
}
