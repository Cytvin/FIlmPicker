using FIlmPicker.Models;
using FIlmPicker.Data;
using FIlmPicker.Data.Models;
using Microsoft.EntityFrameworkCore;
using FIlmPicker.Models.Converters;

namespace FIlmPicker.Services.DatabaseServices
{
    public class MovieService
    {
        private readonly ILogger<MovieService> _logger;
        private readonly ApplicationDbContext _context;
        private readonly GenreService _genreService;

        public MovieService(ILogger<MovieService> logger, ApplicationDbContext context, GenreService genreService)
        {
            _logger = logger;
            _context = context;
            _genreService = genreService;
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
                    genres.Add(await _genreService.GetGenresRecordsByNameAsync(genre.Name));
                }

                movieRecord.Genres = genres;

                moviesRecords.Add(movieRecord);
            }

            _context.Movies.AddRange(moviesRecords);

            _logger.LogInformation("{count} new movie in database", moviesRecords.Count);

            await _context.SaveChangesAsync();
        }

        public async Task<Movie?> GetMovieFromRoomAsync(int id, string roomId)
        {
            RoomMovieEntity? roomMovie = await _context.RoomMovies
                .Include(rm => rm.Movie)
                .ThenInclude(m => m.Genres)
                .FirstOrDefaultAsync(rm => rm.MovieId == id && rm.RoomId == roomId);

            if (roomMovie == null)
            {
                return null;
            }

            return roomMovie.ToModel();
        }

        public async Task SaveMovieScoreAsync(Movie movie)
        {
            RoomMovieEntity? roomMovie = await _context.RoomMovies
                .Include(rm => rm.Movie)
                .FirstOrDefaultAsync(rm => rm.MovieId == movie.Id && rm.RoomId == movie.RoomId);

            if (roomMovie == null)
            {
                return;
            }

            roomMovie.OwnerScore = (int)movie.OwnerScore;
            roomMovie.GuestScore = (int)movie.GuestScore;

            _context.RoomMovies.Update(roomMovie);
            await _context.SaveChangesAsync();
        }

        public async Task<Movie?> GetUnscoredMovieInRoomAsync(string roomId, string userId)
        {
            RoomEntity? room = await _context.Rooms.FindAsync(roomId);

            if (room == null)
            {
                return null;
            }

            if (room.OwnerId == userId)
            {
                RoomMovieEntity? ownerUnscoredmovie = await _context.RoomMovies
                    .Include(rm => rm.Movie)
                    .ThenInclude(m => m.Genres)
                    .FirstOrDefaultAsync(rm => rm.RoomId == room.Id && rm.OwnerScore == 0);

                if (ownerUnscoredmovie == null)
                {
                    return null;
                }

                return ownerUnscoredmovie.ToModel();
            }

            RoomMovieEntity? guestUnscoredMovie = await _context.RoomMovies
                    .Include(rm => rm.Movie)
                    .ThenInclude(m => m.Genres)
                    .FirstOrDefaultAsync(rm => rm.RoomId == room.Id && rm.GuestScore == 0);

            if (guestUnscoredMovie == null)
            {
                return null;
            }

            return guestUnscoredMovie.ToModel();
        }

        public async Task RemoveUnscoredMovieFromRoomAsync(string roomId)
        {
            RoomEntity? room = await _context.Rooms.FindAsync(roomId);

            if (room == null)
            {
                return;
            }

            IEnumerable<RoomMovieEntity> roomMovies = await _context.RoomMovies.
                Where(rm => rm.RoomId == room.Id && rm.OwnerScore == 0 && rm.GuestScore == 0)
                .ToArrayAsync();

            foreach (RoomMovieEntity roomMovie in roomMovies)
            {
                _context.RoomMovies.Remove(roomMovie);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Movie>> GetMoviesInRoomAsync(string roomId)
        {
            IEnumerable<RoomMovieEntity> roomMovies = await _context.RoomMovies
                .Where(rm => rm.RoomId == roomId)
                .Include(rm => rm.Movie)
                .ThenInclude(m => m.Genres)
                .ToListAsync();

            return roomMovies.Select(rm => rm.ToModel());
        }
    }
}
