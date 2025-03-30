using FIlmPicker.Data;
using FIlmPicker.Data.Models;
using FIlmPicker.Models;
using FIlmPicker.Services.DatabaseServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FIlmPicker.Tests
{
    public class MovieServiceTests
    {
        private readonly Mock<ILogger<MovieService>> _loggerMock;
        private readonly Mock<ILogger<GenreService>> _loggerGenreMock;
        private readonly Mock<GenreService> _genreServiceMock;

        public MovieServiceTests()
        {
            _loggerMock = new Mock<ILogger<MovieService>>();
            _loggerGenreMock = new Mock<ILogger<GenreService>>();
            _genreServiceMock = new Mock<GenreService>(MockBehavior.Strict, null, null);
        }

        private ApplicationDbContext GetInMemoryDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            var context = new ApplicationDbContext(options);

            return context;
        }

        [Fact]
        public async Task SaveMovieListAsync_SavesNewMovies()
        {
            // Arrange
            var context = GetInMemoryDbContext("SaveMovieListAsyncDb");
            var genreService = new GenreService(_loggerGenreMock.Object, context);   
            var movieService = new MovieService(_loggerMock.Object, context, genreService);

            var movies = new List<Movie>
            {
                new Movie(1, "Room1", "Movie1", "Description1", 1, 120, 2021, 8.5, 7.5, "Poster1", UserScore.None, UserScore.None, new List<Genre> { new Genre("1", "Action") }),
                new Movie(2, "Room1", "Movie2", "Description2", 1, 130, 2022, 8.0, 7.0, "Poster2", UserScore.None, UserScore.None, new List<Genre> { new Genre("2", "Comedy") })
            };

            // Act
            await movieService.SaveMovieListAsync(movies);

            // Assert
            var savedMovies = await context.Movies.Include(m => m.Genres).ToListAsync();
            Assert.Equal(2, savedMovies.Count);
            Assert.Contains(savedMovies, m => m.Name == "Movie1");
            Assert.Equal("Action", savedMovies[0].Genres.First().Name);
            Assert.Contains(savedMovies, m => m.Name == "Movie2");
            Assert.Equal("Comedy", savedMovies[1].Genres.First().Name);
        }

        [Fact]
        public async Task GetMovieFromRoomAsync_ReturnsMovie_WhenMovieExistsInRoom()
        {
            // Arrange
            var context = GetInMemoryDbContext("GetMovieFromRoomAsyncDb");
            var movieService = new MovieService(_loggerMock.Object, context, _genreServiceMock.Object);

            var movieEntity = new MovieEntity { Id = 1, Name = "Movie1" };
            var roomMovieEntity = new RoomMovieEntity { RoomId = "Room1", MovieId = 1, Movie = movieEntity };

            context.Movies.Add(movieEntity);
            context.RoomMovies.Add(roomMovieEntity);
            await context.SaveChangesAsync();

            // Act
            var result = await movieService.GetMovieFromRoomAsync(1, "Room1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Movie1", result.Name);
        }

        [Fact]
        public async Task GetMovieFromRoomAsync_ReturnsNull_WhenMovieDoesNotExistInRoom()
        {
            // Arrange
            var context = GetInMemoryDbContext("GetMovieFromRoomAsyncDb_NotExist");
            var movieService = new MovieService(_loggerMock.Object, context, _genreServiceMock.Object);

            // Act
            var result = await movieService.GetMovieFromRoomAsync(1, "Room1");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task SaveMovieScoreAsync_UpdatesMovieScore()
        {
            // Arrange
            var context = GetInMemoryDbContext("SaveMovieScoreAsyncDb");
            var movieService = new MovieService(_loggerMock.Object, context, _genreServiceMock.Object);

            var movieEntity = new MovieEntity { Id = 1, Name = "Movie1" };
            var roomMovieEntity = new RoomMovieEntity { RoomId = "Room1", MovieId = 1, Movie = movieEntity, OwnerScore = 0, GuestScore = 0 };

            context.Movies.Add(movieEntity);
            context.RoomMovies.Add(roomMovieEntity);
            await context.SaveChangesAsync();

            var movie = new Movie(1, "Room1", "Movie1", "Description1", 1, 120, 2021, 8.5, 7.5, "Poster1", UserScore.Like, UserScore.Dislike, new List<Genre> { new Genre("Action") });

            // Act
            await movieService.SaveMovieScoreAsync(movie);

            // Assert
            var updatedRoomMovie = await context.RoomMovies.FirstOrDefaultAsync(rm => rm.MovieId == 1 && rm.RoomId == "Room1");
            Assert.NotNull(updatedRoomMovie);
            Assert.Equal(1, updatedRoomMovie.OwnerScore);
            Assert.Equal(2, updatedRoomMovie.GuestScore);
        }

        [Fact]
        public async Task GetUnscoredMovieInRoomAsync_ReturnsUnscoredMovie_ForOwner()
        {
            // Arrange
            var context = GetInMemoryDbContext("GetUnscoredMovieInRoomAsyncDb");
            var movieService = new MovieService(_loggerMock.Object, context, _genreServiceMock.Object);

            var roomEntity = new RoomEntity { Id = "Room1", OwnerId = "Owner1", GuestId = "Guest1" };
            var movieEntity = new MovieEntity { Id = 1, Name = "Movie1" };
            var roomMovieEntity = new RoomMovieEntity { RoomId = "Room1", MovieId = 1, Movie = movieEntity, OwnerScore = 0, GuestScore = 2 };

            context.Rooms.Add(roomEntity);
            context.Movies.Add(movieEntity);
            context.RoomMovies.Add(roomMovieEntity);
            await context.SaveChangesAsync();

            // Act
            var result = await movieService.GetUnscoredMovieInRoomAsync("Room1", "Owner1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Movie1", result.Name);
        }

        [Fact]
        public async Task GetUnscoredMovieInRoomAsync_ReturnsUnscoredMovie_ForGuest()
        {
            // Arrange
            var context = GetInMemoryDbContext("GetUnscoredMovieInRoomAsyncDb1");
            var movieService = new MovieService(_loggerMock.Object, context, _genreServiceMock.Object);

            var roomEntity = new RoomEntity { Id = "Room1", OwnerId = "Owner1", GuestId = "Guest1" };
            var movieEntity = new MovieEntity { Id = 1, Name = "Movie1" };
            var roomMovieEntity = new RoomMovieEntity { RoomId = "Room1", MovieId = 1, Movie = movieEntity, OwnerScore = 1, GuestScore = 0 };

            context.Rooms.Add(roomEntity);
            context.Movies.Add(movieEntity);
            context.RoomMovies.Add(roomMovieEntity);
            await context.SaveChangesAsync();

            // Act
            var result = await movieService.GetUnscoredMovieInRoomAsync("Room1", "Guest1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Movie1", result.Name);
        }

        [Fact]
        public async Task RemoveUnscoredMovieFromRoomAsync_RemovesUnscoredMovies()
        {
            // Arrange
            var context = GetInMemoryDbContext("RemoveUnscoredMovieFromRoomAsyncDb");
            var movieService = new MovieService(_loggerMock.Object, context, _genreServiceMock.Object);

            var roomEntity = new RoomEntity { Id = "Room1", OwnerId = "1", GuestId = "2" };
            var movieEntity = new MovieEntity { Id = 1, Name = "Movie1" };
            var roomMovieEntity = new RoomMovieEntity { RoomId = "Room1", MovieId = 1, Movie = movieEntity, OwnerScore = 0, GuestScore = 0 };

            context.Rooms.Add(roomEntity);
            context.Movies.Add(movieEntity);
            context.RoomMovies.Add(roomMovieEntity);
            await context.SaveChangesAsync();

            // Act
            await movieService.RemoveUnscoredMovieFromRoomAsync("Room1");

            // Assert
            var remainingRoomMovies = await context.RoomMovies.Where(rm => rm.RoomId == "Room1").ToListAsync();
            Assert.Empty(remainingRoomMovies);
        }

        [Fact]
        public async Task GetMoviesInRoomAsync_ReturnsMoviesInRoom()
        {
            // Arrange
            var context = GetInMemoryDbContext("GetMoviesInRoomAsyncDb");
            var movieService = new MovieService(_loggerMock.Object, context, _genreServiceMock.Object);

            var roomEntity = new RoomEntity { Id = "Room1", OwnerId = "1", GuestId = "2" };
            var movieEntity1 = new MovieEntity { Id = 1, Name = "Movie1" };
            var movieEntity2 = new MovieEntity { Id = 2, Name = "Movie2" };
            var roomMovieEntity1 = new RoomMovieEntity { RoomId = "Room1", MovieId = 1, Movie = movieEntity1 };
            var roomMovieEntity2 = new RoomMovieEntity { RoomId = "Room1", MovieId = 2, Movie = movieEntity2 };

            context.Rooms.Add(roomEntity);
            context.Movies.AddRange(movieEntity1, movieEntity2);
            context.RoomMovies.AddRange(roomMovieEntity1, roomMovieEntity2);
            await context.SaveChangesAsync();

            // Act
            var result = await movieService.GetMoviesInRoomAsync("Room1");

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains(result, m => m.Name == "Movie1");
            Assert.Contains(result, m => m.Name == "Movie2");
        }
    }
}