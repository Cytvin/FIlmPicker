using FIlmPicker.Data;
using FIlmPicker.Data.Models;
using FIlmPicker.Models.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FIlmPicker.Services
{
    public class DatabaseService
    {
        private readonly ApplicationDbContext _context;

        public DatabaseService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<MovieDTO?> GetMovieFromRoomAsync(int id, string roomId)
        {
            RoomMovie? roomMovie = await _context.RoomMovies
                .Include(rm => rm.Movie)
                .FirstOrDefaultAsync(rm => rm.MovieId == id && rm.RoomId == roomId);

            if (roomMovie == null)
            {
                return null;
            }

            roomMovie.Movie.Genres = await _context.MovieGenres
                .Include(mg => mg.Genre)
                .Where(mg => mg.MovieId == roomMovie.MovieId)
                .ToListAsync();

            return ConvertRoomMovieToDTO(roomMovie);
        }

        public async Task SaveMovieScoreAsync(MovieDTO movieDTO)
        {
            RoomMovie? roomMovie = await _context.RoomMovies
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

        public async Task SaveMovieAsync(MovieDTO movie)
        {
            Movie? existingRecord = await _context.Movies.FindAsync(movie.Id);

            if (existingRecord != null)
            {
                return;
            }

            Movie movieRecord = new Movie()
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

            List<Genre> genres = new List<Genre>();

            foreach (var genre in movie.Genres)
            {
                genres.Add(await GetGenresRecordsByNameAsync(genre.Name));
            }

            movieRecord.Genres = genres
                .Select(g => new MovieGenre { GenreId = g.Id, MovieId = movieRecord.Id}).ToList();

            _context.Movies.Add(movieRecord);
            await _context.SaveChangesAsync();
        }

        public async Task AddMovieToRoomAsync(int movieId, string roomId)
        {
            Room? room = await _context.Rooms.FindAsync(roomId);

            if (room == null)
            {
                return;
            }

            RoomMovie roomMovie = new RoomMovie
            {
                MovieId = movieId,
                RoomId = roomId,
                OwnerScore = 0,
                GuestScore = 0
            };

            _context.RoomMovies.Add(roomMovie);
            await _context.SaveChangesAsync();
        }

        public async Task<UserDTO?> GetUserByUserNameAsync(string userName)
        {
            string userNameNormalized = userName.Trim().ToUpper();

            IdentityUser? user = await _context.Users.FirstOrDefaultAsync(u => String.Equals(u.NormalizedUserName, userNameNormalized));

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

        public async Task<RoomDTO?> IsRoomWithUsersExistAsync(string ownerId, string guestId) 
        {
            Room? room;

            room = await _context.Rooms
                .Include(r => r.Owner)
                .Include(r => r.Guest)
                .Include(r => r.RoomSetting)
                .ThenInclude(rs => rs.Genres)
                .ThenInclude(rsg => rsg.Genre)
                .FirstOrDefaultAsync(r => r.OwnerId == ownerId && r.GuestId == guestId);

            if (room == null)
            {
                return null;
            }

            return ConvertRoomToDTO(room);
        }

        public async Task SaveRoomAsync(RoomDTO room)
        {
            Room roomRecord = new Room
            {
                Id = room.Id,
                OwnerId = room.Owner.Id,
                GuestId = room.Guest.Id,
                InviteAccepted = room.InviteAccepted
            };

            RoomSettings roomSettings = new RoomSettings()
            {
                RoomId = room.Id,
                MinKpRating = room.Settings.MinKpRaitings,
                MaxKpRating = room.Settings.MaxKpRaitings,
                MinYear = room.Settings.MinYear,
                MaxYear = room.Settings.MaxYear,
                TypeNumber = room.Settings.TypeNumber
            };

            roomRecord.RoomSetting = roomSettings;

            _context.Rooms.Add(roomRecord);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRoomAsync(RoomDTO roomDTO)
        {
            Room? room = await _context.Rooms.FindAsync(roomDTO.Id);

            if (room == null)
            {
                return;
            }

            room.InviteAccepted = roomDTO.InviteAccepted;
            room.OwnerIsOut = roomDTO.OwnerIsOut;
            room.GuestIsOut = roomDTO.GuestIsOut;
            
            _context.Update(room);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<RoomDTO>> GetUserOwnRoomsAsync(string userId)
        {
            IEnumerable<Room> userOwnRooms = await _context.Rooms
                .Where(r => r.OwnerId == userId && r.OwnerIsOut == false)
                .Include(r => r.RoomSetting)
                .ThenInclude(rs => rs.Genres)
                .ThenInclude(rsg => rsg.Genre)
                .Include(r => r.Owner)
                .Include(r => r.Guest)
                .ToListAsync();

            return userOwnRooms.Select(ConvertRoomToDTO);
        }

        public async Task<IEnumerable<RoomDTO>> GetUserGuestRoomsAsync(string userId)
        {
            IEnumerable<Room> userGuestRooms = await _context.Rooms
                .Where(r => r.GuestId == userId && r.InviteAccepted && r.GuestIsOut == false)
                .Include(r => r.RoomSetting)
                .ThenInclude(rs => rs.Genres)
                .ThenInclude(rsg => rsg.Genre)
                .Include(r => r.Owner)
                .Include(r => r.Guest)
                .ToListAsync();

            return userGuestRooms.Select(ConvertRoomToDTO);
        }

        public async Task<IEnumerable<RoomDTO>> GetRoomInvitationsAsync(string userId)
        {
            IEnumerable<Room> userRooms = await _context.Rooms
                .Where(r => r.GuestId == userId && !r.InviteAccepted)
                .Include(r => r.RoomSetting)
                .ThenInclude(rs => rs.Genres)
                .ThenInclude(rsg => rsg.Genre)
                .Include(r => r.Owner)
                .Include(r => r.Guest).ToListAsync();

            return userRooms.Select(ConvertRoomToDTO);
        }

        public async Task<int> GetInviteCountAsync(string userId)
        {
            int inviteCount = await _context.Rooms
                .Where(r => r.GuestId == userId && r.InviteAccepted == false)
                .CountAsync();

            return inviteCount;
        }

        public async Task<RoomDTO?> GetRoomAsync(string id)
        {
            Room? room;

            room = await _context.Rooms
                .Include(r => r.RoomSetting)
                .ThenInclude(rs => rs.Genres)
                .ThenInclude(rsg => rsg.Genre)
                .Include(r => r.Owner)
                .Include(r => r.Guest)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (room == null)
            {
                return null;
            }

            return ConvertRoomToDTO(room);
        }

        public async Task DeleteRoomAsync(string id)
        {
            Room? room = await _context.Rooms.FindAsync(id);

            if (room == null)
            {
                return;
            }

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<MovieDTO>> GetUnscoredMovieInRoomAsync(string roomId, string userId)
        {
            Room? room = await _context.Rooms
                .Include(r => r.Movies)
                .ThenInclude(rm => rm.Movie)
                .ThenInclude(m => m.Genres)
                .ThenInclude(mg => mg.Genre)
                .FirstOrDefaultAsync(r => r.Id == roomId);

            if (room == null)
            {
                return new List<MovieDTO>();
            }

            if (room.Owner.Id == userId)
            {
                return room.Movies.Where(r => r.OwnerScore == 0)
                    .Select(ConvertRoomMovieToDTO);
            }

            return room.Movies.Where(r => r.GuestScore == 0)
                    .Select(ConvertRoomMovieToDTO);
        }

        public async Task<IEnumerable<MovieDTO>> GetMoviesInRoomAsync(string roomId)
        {
            IEnumerable<RoomMovie> roomMovies = await _context.RoomMovies
                .Where(rm => rm.RoomId == roomId)
                .Include(rm => rm.Movie)
                .ToListAsync();

            foreach (var item in roomMovies)
            {
                item.Movie.Genres = await _context.MovieGenres
                    .Include(mg => mg.Genre)
                    .Where(mg => mg.MovieId == item.MovieId)
                    .ToListAsync();
            }

            return roomMovies.Select(ConvertRoomMovieToDTO);
        }

        public async Task<RoomSettingsDTO?> GetRoomSettingsByIdAsync(string settingsId)
        {
            RoomSettings? roomSettings = await _context.RoomSettings
                .Include(rsg => rsg.Genres)
                .ThenInclude(g => g.Genre)
                .FirstOrDefaultAsync(rs => rs.Id == settingsId);

            if (roomSettings == null)
            {
                return null;
            }

            return ConvertRoomSettingsToDTO(roomSettings);
        }

        public async Task UpdateRoomSettingsAsync(RoomSettingsDTO roomSettings)
        {
            RoomSettings? settings = await _context.RoomSettings.FindAsync(roomSettings.Id);

            if (settings == null)
            {
                return;
            }

            List<Genre> genres = new List<Genre>();

            foreach (var genreDTO in roomSettings.Genres)
            {
                Genre? genre = await _context.Genres.FindAsync(genreDTO.Id);
                
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
            settings.Genres = genres
                .Select(g => new RoomSettingsGenre { RoomSettingsId = settings.Id, GenreId = g.Id })
                .ToList();

            _context.RoomSettings.Update(settings);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<GenreDTO>> GetAllGenresAsync()
        {
            List<Genre> genres = await _context.Genres.ToListAsync();

            return genres.Select(ConvertGenreToDTO);
        }

        public async Task<GenreDTO?> GetGenreByIdAsync(string id)
        {
            Genre? genre = await _context.Genres.FindAsync(id);

            if (genre == null)
            {
                return null;
            }

            return ConvertGenreToDTO(genre);
        }

        private async Task<Genre> GetGenresRecordsByNameAsync(string name)
        {
            Genre? genre = await _context.Genres
                .FirstOrDefaultAsync(x => x.Name == name);

            if (genre != null)
            {
                return genre;
            }

            genre = new Genre() 
            { 
                Name = name 
            };

            _context.Genres.Add(genre);
            await _context.SaveChangesAsync();
            return genre;
        }

        private RoomDTO ConvertRoomToDTO(Room room)
        {
            RoomDTO roomDTO = new RoomDTO
            {
                Id = room.Id,
                Owner = ConvertUserToDTO(room.Owner),
                Guest = ConvertUserToDTO(room.Guest),
                OwnerIsOut = room.OwnerIsOut,
                GuestIsOut = room.GuestIsOut,
                InviteAccepted = room.InviteAccepted,
                Settings = ConvertRoomSettingsToDTO(room.RoomSetting)
            };

            return roomDTO;
        }

        private RoomSettingsDTO ConvertRoomSettingsToDTO(RoomSettings settings)
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

        private MovieDTO ConvertRoomMovieToDTO(RoomMovie roomMovie)
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

        private GenreDTO ConvertGenreToDTO(MovieGenre movieGenre)
        {
            GenreDTO result = new GenreDTO
            {
                Id = movieGenre.Genre.Id,
                Name = movieGenre.Genre.Name,
            };

            return result;
        }

        private GenreDTO ConvertGenreToDTO(RoomSettingsGenre roomSettingsGenre)
        {
            GenreDTO result = new GenreDTO
            {
                Id = roomSettingsGenre.Genre.Id,
                Name = roomSettingsGenre.Genre.Name
            };

            return result;
        }

        private GenreDTO ConvertGenreToDTO(Genre genre)
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