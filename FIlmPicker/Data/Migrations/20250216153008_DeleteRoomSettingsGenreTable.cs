using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FIlmPicker.Data.Migrations
{
    /// <inheritdoc />
    public partial class DeleteRoomSettingsGenreTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoomSettingsGenre");

            migrationBuilder.CreateTable(
                name: "GenreRoomSettings",
                columns: table => new
                {
                    GenresId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoomSettingsId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GenreRoomSettings", x => new { x.GenresId, x.RoomSettingsId });
                    table.ForeignKey(
                        name: "FK_GenreRoomSettings_Genres_GenresId",
                        column: x => x.GenresId,
                        principalTable: "Genres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GenreRoomSettings_RoomSettings_RoomSettingsId",
                        column: x => x.RoomSettingsId,
                        principalTable: "RoomSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GenreRoomSettings_RoomSettingsId",
                table: "GenreRoomSettings",
                column: "RoomSettingsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GenreRoomSettings");

            migrationBuilder.CreateTable(
                name: "RoomSettingsGenre",
                columns: table => new
                {
                    RoomSettingsId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    GenreId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomSettingsGenre", x => new { x.RoomSettingsId, x.GenreId });
                    table.ForeignKey(
                        name: "FK_RoomSettingsGenre_Genres_GenreId",
                        column: x => x.GenreId,
                        principalTable: "Genres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoomSettingsGenre_RoomSettings_RoomSettingsId",
                        column: x => x.RoomSettingsId,
                        principalTable: "RoomSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoomSettingsGenre_GenreId",
                table: "RoomSettingsGenre",
                column: "GenreId");
        }
    }
}
