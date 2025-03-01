using FIlmPicker.Models;
using FIlmPicker.Data;
using FIlmPicker.Data.Models;

namespace FIlmPicker.Services.DatabaseServices
{
    public class MovieService
    {
        private readonly DatabaseService _databaseService;
        private readonly ILogger<DatabaseService> _logger;
        private readonly ApplicationDbContext _context;

        public MovieService(DatabaseService databaseService, ILogger<DatabaseService> logger, ApplicationDbContext context)
        {
            _databaseService = databaseService;
            _logger = logger;
            _context = context;
        }

        public async Task SaveMovieListAsync(IEnumerable<Movie> movieList)
        {
            int existedMovieCount = 0;
            int movieCount = movieList.Count();

            _logger.LogInformation("Movie count: {count}", movieCount);

            List<Movie> filteredMovieList = new List<Movie>();

            foreach (Movie movie in movieList)
            {
                MovieEntity? existingRecord = await _context.Movies.FindAsync(movie.Id);

                if (existingRecord != null)
                {
                    existedMovieCount++;

                    continue;
                }

                filteredMovieList.Add(movie);
            }

            _logger.LogInformation("{count} movie already in database", existedMovieCount);
            _logger.LogInformation("{count} filtered movie", filteredMovieList.Count);

            if (filteredMovieList.Count == 0)
            {
                return;
            }

            List<MovieEntity> moviesRecords = new List<MovieEntity>();

            foreach (Movie movie in filteredMovieList)
            {
                MovieEntity movieRecord = new MovieEntity()
                {
                    Id = movie.Id,
                    Name = movie.Name,
                    Description = movie.Description,
                    Year = movie.Year,
                    MovieLength = movie.MovieLength,
                    ImdbRating = movie.ImdbRaiting,
                    KpRaiting = movie.KpRaiting,
                    TypeId = movie.TypeNumber,
                    Poster = movie.Poster
                };

                List<GenreEntity> genres = new List<GenreEntity>();

                foreach (var genre in movie.Genres)
                {
                    genres.Add(await _databaseService.GetGenresRecordsByNameAsync(genre.Name));
                }

                movieRecord.Genres = genres;

                moviesRecords.Add(movieRecord);
            }

            _context.Movies.AddRange(moviesRecords);

            _logger.LogInformation("{count} new movie in database", moviesRecords.Count);

            await _context.SaveChangesAsync();
        }
    }
}
