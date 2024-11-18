using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FIlmPicker.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameRoomFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OwnerOut",
                table: "Rooms",
                newName: "OwnerIsOut");

            migrationBuilder.RenameColumn(
                name: "GuestOut",
                table: "Rooms",
                newName: "GuestIsOut");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OwnerIsOut",
                table: "Rooms",
                newName: "OwnerOut");

            migrationBuilder.RenameColumn(
                name: "GuestIsOut",
                table: "Rooms",
                newName: "GuestOut");
        }
    }
}
