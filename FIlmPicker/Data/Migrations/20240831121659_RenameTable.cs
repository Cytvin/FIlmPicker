using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FIlmPicker.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MoviesInRoom_Rooms_RoomId",
                table: "MoviesInRoom");

            migrationBuilder.DropForeignKey(
                name: "FK_RoomSettings_Rooms_RoomId",
                table: "RoomSettings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RoomSettings",
                table: "RoomSettings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MoviesInRoom",
                table: "MoviesInRoom");

            migrationBuilder.RenameTable(
                name: "RoomSettings",
                newName: "RoomsSettings");

            migrationBuilder.RenameTable(
                name: "MoviesInRoom",
                newName: "MoviesInRooms");

            migrationBuilder.RenameIndex(
                name: "IX_RoomSettings_RoomId",
                table: "RoomsSettings",
                newName: "IX_RoomsSettings_RoomId");

            migrationBuilder.RenameIndex(
                name: "IX_MoviesInRoom_RoomId",
                table: "MoviesInRooms",
                newName: "IX_MoviesInRooms_RoomId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RoomsSettings",
                table: "RoomsSettings",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MoviesInRooms",
                table: "MoviesInRooms",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MoviesInRooms_Rooms_RoomId",
                table: "MoviesInRooms",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RoomsSettings_Rooms_RoomId",
                table: "RoomsSettings",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MoviesInRooms_Rooms_RoomId",
                table: "MoviesInRooms");

            migrationBuilder.DropForeignKey(
                name: "FK_RoomsSettings_Rooms_RoomId",
                table: "RoomsSettings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RoomsSettings",
                table: "RoomsSettings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MoviesInRooms",
                table: "MoviesInRooms");

            migrationBuilder.RenameTable(
                name: "RoomsSettings",
                newName: "RoomSettings");

            migrationBuilder.RenameTable(
                name: "MoviesInRooms",
                newName: "MoviesInRoom");

            migrationBuilder.RenameIndex(
                name: "IX_RoomsSettings_RoomId",
                table: "RoomSettings",
                newName: "IX_RoomSettings_RoomId");

            migrationBuilder.RenameIndex(
                name: "IX_MoviesInRooms_RoomId",
                table: "MoviesInRoom",
                newName: "IX_MoviesInRoom_RoomId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RoomSettings",
                table: "RoomSettings",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MoviesInRoom",
                table: "MoviesInRoom",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MoviesInRoom_Rooms_RoomId",
                table: "MoviesInRoom",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RoomSettings_Rooms_RoomId",
                table: "RoomSettings",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
