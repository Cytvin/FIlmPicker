using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FIlmPicker.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBasicTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    OwnerId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    GuestId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    InviteAccepted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rooms_AspNetUsers_GuestId",
                        column: x => x.GuestId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Rooms_AspNetUsers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MoviesInRoom",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoomId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MovieKpId = table.Column<int>(type: "int", nullable: false),
                    OwnerScore = table.Column<int>(type: "int", nullable: false),
                    GuestScore = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MoviesInRoom", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MoviesInRoom_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoomSettings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoomId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MinKpRating = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaxKpRating = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MinYear = table.Column<int>(type: "int", nullable: false),
                    MaxYear = table.Column<int>(type: "int", nullable: false),
                    TypeNumber = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoomSettings_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MoviesInRoom_RoomId",
                table: "MoviesInRoom",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_GuestId",
                table: "Rooms",
                column: "GuestId");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_OwnerId",
                table: "Rooms",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomSettings_RoomId",
                table: "RoomSettings",
                column: "RoomId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MoviesInRoom");

            migrationBuilder.DropTable(
                name: "RoomSettings");

            migrationBuilder.DropTable(
                name: "Rooms");
        }
    }
}
