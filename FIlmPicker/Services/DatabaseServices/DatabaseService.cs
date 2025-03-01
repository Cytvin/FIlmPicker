using FIlmPicker.Data;
using FIlmPicker.Data.Models;
using FIlmPicker.Models.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FIlmPicker.Services.DatabaseServices
{
    public class DatabaseService
    {
        private readonly ILogger<DatabaseService> _logger;
        private readonly ApplicationDbContext _context;
        private readonly RoomService _roomService;
        private readonly MovieService _movieService;

        public RoomService RoomService => _roomService;
        public MovieService MovieService => _movieService;

        public DatabaseService(ILogger<DatabaseService> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
            _roomService = new RoomService(logger, context);
            _movieService = new MovieService(this, logger, context);
        }

        public async Task<MovieDTO?> GetMovieFromRoomAsync(int id, string roomId)
        {
            RoomMovieEntity? roomMovie = await _context.RoomMovies
                .Include(rm => rm.Movie)
                .ThenInclude(m => m.Genres)
                .FirstOrDefaultAsync(rm => rm.MovieId == id && rm.RoomId == roomId);

            if (roomMovie == null)
            {
                return null;
            }

            return ConvertRoomMovieToDTO(roomMovie);
        }

        public async Task SaveMovieScoreAsync(MovieDTO movieDTO)
        {
            RoomMovieEntity? roomMovie = await _context.RoomMovies
                .Include(rm => rm.Movie)
                .FirstOrDefaultAsync(rm => rm.MovieId == movieDTO.Id && rm.RoomId == movieDTO.RoomId);

            if (roomMovie == null)
            {
                return;
            }

            roomMovie.OwnerScore = movieDTO.OwnerScore;
            roomMovie.GuestScore = movieDTO.GuestScore;

            _context.RoomMovies.Update(roomMovie);
            await _context.SaveChangesAsync();
        }
        public async Task<UserDTO?> GetUserByUserNameAsync(string userName)
        {
            string userNameNormalized = userName.Trim().ToUpper();

            IdentityUser? user = await _context.Users.FirstOrDefaultAsync(u => string.Equals(u.NormalizedUserName, userNameNormalized));

            if (user == null)
            {
                return null;
            }

            return ConvertUserToDTO(user);
        }

        public async Task<UserDTO?> GetUserByIdAsync(string id)
        {
            IdentityUser? user = await _context.Users.FindAsync(id);
            
            if (user == null)
            {
                return null;
            }

            return ConvertUserToDTO(user);
        }

        public async Task<MovieDTO?> GetUnscoredMovieInRoomAsync(string roomId, string userId)
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

                return ConvertRoomMovieToDTO(ownerUnscoredmovie);
            }

            RoomMovieEntity? guestUnscoredMovie = await _context.RoomMovies
                    .Include(rm => rm.Movie)
                    .ThenInclude(m => m.Genres)
                    .FirstOrDefaultAsync(rm => rm.RoomId == room.Id && rm.GuestScore == 0);

            if (guestUnscoredMovie == null)
            {
                return null;
            }

            return ConvertRoomMovieToDTO(guestUnscoredMovie);
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

            foreach(RoomMovieEntity roomMovie in roomMovies)
            {
                _context.RoomMovies.Remove(roomMovie);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<MovieDTO>> GetMoviesInRoomAsync(string roomId)
        {
            IEnumerable<RoomMovieEntity> roomMovies = await _context.RoomMovies
                .Where(rm => rm.RoomId == roomId)
                .Include(rm => rm.Movie)
                .ThenInclude(m => m.Genres)
                .ToListAsync();

            return roomMovies.Select(ConvertRoomMovieToDTO);
        }

        public async Task<RoomSettingsDTO?> GetRoomSettingsByIdAsync(string settingsId)
        {
            RoomSettingsEntity? roomSettings = await _context.RoomSettings
                .Include(rsg => rsg.Genres)
                .FirstOrDefaultAsync(rs => rs.Id == settingsId);

            if (roomSettings == null)
            {
                return null;
            }

            return ConvertRoomSettingsToDTO(roomSettings);
        }

        public async Task UpdateRoomSettingsAsync(RoomSettingsDTO roomSettings)
        {
            RoomSettingsEntity? settings = await _context.RoomSettings
                .Include(rs => rs.Genres)
                .FirstOrDefaultAsync(rs => rs.Id == roomSettings.Id);

            if (settings == null)
            {
                return;
            }

            List<GenreEntity> genres = new List<GenreEntity>();

            foreach (var genreDTO in roomSettings.Genres)
            {
                GenreEntity? genre = await _context.Genres.FindAsync(genreDTO.Id);
                
                if (genre == null)
                {
                    continue;
                }

                genres.Add(genre);
            }

            settings.RoomId = roomSettings.RoomId;
            settings.MinKpRating = roomSettings.MinKpRaitings;
            settings.MaxKpRating = roomSettings.MaxKpRaitings;
            settings.MinYear = roomSettings.MinYear;
            settings.MaxYear = roomSettings.MaxYear;
            settings.TypeNumber = roomSettings.TypeNumber;
            settings.MoviesReceived = roomSettings.MoviesReceived;
            settings.Genres = genres;

            _context.RoomSettings.Update(settings);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<GenreDTO>> GetAllGenresAsync()
        {
            List<GenreEntity> genres = await _context.Genres.ToListAsync();

            return genres.Select(ConvertGenreToDTO);
        }

        public async Task<GenreDTO?> GetGenreByIdAsync(string id)
        {
            GenreEntity? genre = await _context.Genres.FindAsync(id);

            if (genre == null)
            {
                return null;
            }

            return ConvertGenreToDTO(genre);
        }

        public async Task CreateMovieListOnUpdate(string roomId)
        {
            RoomEntity? room = await _context.Rooms.FindAsync(roomId);

            if (room == null)
            {
                return;
            }

            MovieListOnUpdate movieListOnUpdate = new MovieListOnUpdate()
            {
                RoomId = roomId
            };

            _context.MovieListsOnUpdate.Add(movieListOnUpdate);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsMovieListOnUpdate(string roomId)
        {
            RoomEntity? room = await _context.Rooms.FindAsync(roomId);

            if (room == null)
            {
                return false;
            }

            return await _context.MovieListsOnUpdate.AnyAsync(ml => ml.RoomId == room.Id);
        }

        public async Task DeleteMovieListOnUpdate(string roomId)
        {
            RoomEntity? room = await _context.Rooms.FindAsync(roomId);

            if (room == null)
            {
                return;
            }

            await _context.MovieListsOnUpdate.Where(ml => ml.RoomId == room.Id).ExecuteDeleteAsync();
        }

        public async Task<GenreEntity> GetGenresRecordsByNameAsync(string name)
        {
            GenreEntity? genre = await _context.Genres
                .FirstOrDefaultAsync(x => x.Name == name);

            if (genre != null)
            {
                return genre;
            }

            genre = new GenreEntity() 
            { 
                Name = name 
            };

            _context.Genres.Add(genre);
            await _context.SaveChangesAsync();
            return genre;
        }

        //private RoomDTO ConvertRoomToDTO(RoomEntity room)
        //{
        //    RoomDTO roomDTO = new RoomDTO
        //    {
        //        Id = room.Id,
        //        Owner = ConvertUserToDTO(room.Owner),
        //        Guest = ConvertUserToDTO(room.Guest),
        //        OwnerIsOut = room.OwnerIsOut,
        //        GuestIsOut = room.GuestIsOut,
        //        InviteAccepted = room.InviteAccepted,
        //        Settings = ConvertRoomSettingsToDTO(room.RoomSetting)
        //    };

        //    return roomDTO;
        //}

        private RoomSettingsDTO ConvertRoomSettingsToDTO(RoomSettingsEntity settings)
        {
            RoomSettingsDTO settingsDTO = new RoomSettingsDTO
            {
                Id = settings.Id,
                RoomId = settings.RoomId,
                MinKpRaitings = settings.MinKpRating,
                MaxKpRaitings = settings.MaxKpRating,
                MinYear = settings.MinYear,
                MaxYear = settings.MaxYear,
                TypeNumber = settings.TypeNumber,
                MoviesReceived = settings.MoviesReceived,
                Genres = settings.Genres.Select(ConvertGenreToDTO)
            };

            return settingsDTO;
        }

        private UserDTO ConvertUserToDTO(IdentityUser user)
        {
            UserDTO userDTO = new UserDTO
            {
                Id = user.Id,
                UserName = user.UserName
            };

            return userDTO;
        }

        private MovieDTO ConvertRoomMovieToDTO(RoomMovieEntity roomMovie)
        {
            MovieDTO result = new MovieDTO
            {
                Id = roomMovie.Movie.Id,
                RoomId = roomMovie.RoomId,
                Name = roomMovie.Movie.Name,
                Description = roomMovie.Movie.Description,
                Year = roomMovie.Movie.Year,
                ImdbRaiting = roomMovie.Movie.ImdbRating,
                KpRaiting = roomMovie.Movie.KpRaiting,
                MovieLength = roomMovie.Movie.MovieLength,
                TypeNumber = roomMovie.Movie.TypeId,
                OwnerScore = roomMovie.OwnerScore,
                GuestScore = roomMovie.GuestScore,
                Poster = roomMovie.Movie.Poster,
                Genres = roomMovie.Movie.Genres.Select(ConvertGenreToDTO).ToList()
            };

            return result;
        }

        private GenreDTO ConvertGenreToDTO(GenreEntity genre)
        {
            GenreDTO result = new GenreDTO
            { 
                Id = genre.Id,
                Name = genre.Name
            };

            return result;
        }
    }
}