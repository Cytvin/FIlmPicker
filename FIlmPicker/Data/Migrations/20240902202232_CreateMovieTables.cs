using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FIlmPicker.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateMovieTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoomsSettings_Rooms_RoomId",
                table: "RoomsSettings");

            migrationBuilder.DropTable(
                name: "MoviesInRooms");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RoomsSettings",
                table: "RoomsSettings");

            migrationBuilder.RenameTable(
                name: "RoomsSettings",
                newName: "RoomSettings");

            migrationBuilder.RenameIndex(
                name: "IX_RoomsSettings_RoomId",
                table: "RoomSettings",
                newName: "IX_RoomSettings_RoomId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RoomSettings",
                table: "RoomSettings",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Genres",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genres", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Types",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Types", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Movies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TypeId = table.Column<int>(type: "int", nullable: false),
                    MovieLength = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    KpRaiting = table.Column<double>(type: "float", nullable: false),
                    ImdbRating = table.Column<double>(type: "float", nullable: false),
                    Poster = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Movies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Movies_Types_TypeId",
                        column: x => x.TypeId,
                        principalTable: "Types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MovieGenres",
                columns: table => new
                {
                    MovieId = table.Column<int>(type: "int", nullable: false),
                    GenreId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovieGenres", x => new { x.MovieId, x.GenreId });
                    table.ForeignKey(
                        name: "FK_MovieGenres_Genres_GenreId",
                        column: x => x.GenreId,
                        principalTable: "Genres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MovieGenres_Movies_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoomMovies",
                columns: table => new
                {
                    RoomId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MovieId = table.Column<int>(type: "int", nullable: false),
                    OwnerScore = table.Column<int>(type: "int", nullable: false),
                    GuestScore = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomMovies", x => new { x.RoomId, x.MovieId });
                    table.ForeignKey(
                        name: "FK_RoomMovies_Movies_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoomMovies_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MovieGenres_GenreId",
                table: "MovieGenres",
                column: "GenreId");

            migrationBuilder.CreateIndex(
                name: "IX_Movies_TypeId",
                table: "Movies",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomMovies_MovieId",
                table: "RoomMovies",
                column: "MovieId");

            migrationBuilder.AddForeignKey(
                name: "FK_RoomSettings_Rooms_RoomId",
                table: "RoomSettings",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoomSettings_Rooms_RoomId",
                table: "RoomSettings");

            migrationBuilder.DropTable(
                name: "MovieGenres");

            migrationBuilder.DropTable(
                name: "RoomMovies");

            migrationBuilder.DropTable(
                name: "Genres");

            migrationBuilder.DropTable(
                name: "Movies");

            migrationBuilder.DropTable(
                name: "Types");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RoomSettings",
                table: "RoomSettings");

            migrationBuilder.RenameTable(
                name: "RoomSettings",
                newName: "RoomsSettings");

            migrationBuilder.RenameIndex(
                name: "IX_RoomSettings_RoomId",
                table: "RoomsSettings",
                newName: "IX_RoomsSettings_RoomId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RoomsSettings",
                table: "RoomsSettings",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "MoviesInRooms",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoomId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    GuestScore = table.Column<int>(type: "int", nullable: false),
                    MovieKpId = table.Column<int>(type: "int", nullable: false),
                    OwnerScore = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MoviesInRooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MoviesInRooms_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MoviesInRooms_RoomId",
                table: "MoviesInRooms",
                column: "RoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_RoomsSettings_Rooms_RoomId",
                table: "RoomsSettings",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
