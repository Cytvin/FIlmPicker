using FIlmPicker.Data;
using FIlmPicker.Data.Models;
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
    public class GenreServiceTests
    {
        private readonly Mock<ILogger<GenreService>> _loggerMock;

        public GenreServiceTests()
        {
            _loggerMock = new Mock<ILogger<GenreService>>();
        }

        private ApplicationDbContext GetInMemoryDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            var context = new ApplicationDbContext(options);

            // Инициализация данных
            context.Genres.AddRange(new List<GenreEntity>
            {
                new GenreEntity { Id = "1", Name = "Action" },
                new GenreEntity { Id = "2", Name = "Comedy" }
            });
            context.SaveChanges();

            return context;
        }

        [Fact]
        public async Task GetAllGenresAsync_ReturnsAllGenres()
        {
            // Arrange
            var context = GetInMemoryDbContext("GetAllGenresAsyncDb");
            var genreService = new GenreService(_loggerMock.Object, context);

            // Act
            var result = await genreService.GetAllGenresAsync();

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains(result, g => g.Name == "Action");
            Assert.Contains(result, g => g.Name == "Comedy");
        }

        [Fact]
        public async Task GetGenreByIdAsync_ReturnsGenre_WhenGenreExists()
        {
            // Arrange
            var context = GetInMemoryDbContext("GetGenreByIdAsyncDb");
            var genreService = new GenreService(_loggerMock.Object, context);

            // Act
            var result = await genreService.GetGenreByIdAsync("1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Action", result.Name);
        }

        [Fact]
        public async Task GetGenreByIdAsync_ReturnsNull_WhenGenreDoesNotExist()
        {
            // Arrange
            var context = GetInMemoryDbContext("GetGenreByIdAsyncDb_NotExist");
            var genreService = new GenreService(_loggerMock.Object, context);

            // Act
            var result = await genreService.GetGenreByIdAsync("3");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetGenresRecordsByNameAsync_ReturnsExistingGenre_WhenGenreExists()
        {
            // Arrange
            var context = GetInMemoryDbContext("GetGenresRecordsByNameAsyncDb");
            var genreService = new GenreService(_loggerMock.Object, context);

            // Act
            var result = await genreService.GetGenresRecordsByNameAsync("Action");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("1", result.Id);
            Assert.Equal("Action", result.Name);
        }

        [Fact]
        public async Task GetGenresRecordsByNameAsync_CreatesAndReturnsNewGenre_WhenGenreDoesNotExist()
        {
            // Arrange
            var context = GetInMemoryDbContext("GetGenresRecordsByNameAsyncDb_NotExist");
            var genreService = new GenreService(_loggerMock.Object, context);

            // Act
            var result = await genreService.GetGenresRecordsByNameAsync("Drama");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Drama", result.Name);
            var genreInDb = await context.Genres.FirstOrDefaultAsync(x => x.Name == "Drama");
            Assert.NotNull(genreInDb);
        }
    }
}