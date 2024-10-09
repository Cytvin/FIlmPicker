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

        public MovieDTO? GetMovieById(int id)
        {
            Movie? movie = _context.Movies
                .Include(m => m.Genres)
                .FirstOrDefault(x => x.Id == id);

            if (movie == null) 
            {
                return null;
            }

            return ConvertMovieToDTO(movie);
        }

        public MovieDTO? GetMovieFromRoom(int id, string roomId)
        {
            RoomMovie? roomMovie = _context.RoomMovies
                .Include(rm => rm.Movie)
                .FirstOrDefault(rm => rm.MovieId == id && rm.RoomId == roomId);

            if (roomMovie == null)
            {
                return null;
            }

            IEnumerable<MovieGenre> movieGenres = _context.MovieGenres
                .Include(mg => mg.Genre)
                .Where(mg => mg.MovieId == roomMovie.MovieId);

            roomMovie.Movie.Genres = movieGenres.ToList();

            return ConvertRoomMovieToDTO(roomMovie);
        }

        public void SaveMovieScore(MovieDTO movieDTO)
        {
            RoomMovie? roomMovie = _context.RoomMovies
                .Include(rm => rm.Movie)
                .FirstOrDefault(rm => rm.MovieId == movieDTO.Id && rm.RoomId == movieDTO.RoomId);

            if (roomMovie == null)
            {
                return;
            }

            roomMovie.OwnerScore = movieDTO.OwnerScore;
            roomMovie.GuestScore = movieDTO.GuestScore;

            _context.RoomMovies.Update(roomMovie);
            _context.SaveChanges();
        }

        public void SaveMovie(MovieDTO movie)
        {
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

            RoomMovie roomMovie = new RoomMovie()
            {
                MovieId = movie.Id,
                RoomId = movie.RoomId,
                OwnerScore = movie.OwnerScore,
                GuestScore = movie.GuestScore
            };

            IEnumerable<Genre> genres = movie.Genres
                .Select(g => GetGenresRecordsByName(g.Name));

            movieRecord.Genres = genres
                .Select(g => new MovieGenre { GenreId = g.Id, MovieId = movieRecord.Id}).ToList();

            _context.Movies.Add(movieRecord);
            _context.RoomMovies.Add(roomMovie);
            _context.SaveChanges();
        }

        public IEnumerable<GenreDTO> GetAllGenres()
        {
            IEnumerable<Genre> genres = _context.Genres;

            return genres.Select(ConvertGenreToDTO);
        }

        public IEnumerable<RoomDTO> GetUserRoomsById(string userId)
        {
            IEnumerable<Room> userRooms = _context.Rooms
                .Where(r => r.OwnerId == userId || r.GuestId == userId)
                .Include(r => r.RoomSetting)
                .Include(r => r.Owner)
                .Include(r => r.Guest);

            return userRooms.Select(ConvertRoomToDTO);
        }

        public UserDTO? GetUserByUserName(string userName)
        {
            IdentityUser? user = _context.Users.FirstOrDefault(u => u.UserName == userName);

            if (user == null)
            {
                return null;
            }

            return ConvertUserToDTO(user);
        }

        public UserDTO? GetUserById(string id)
        {
            IdentityUser? user = _context.Users.Find(id);
            
            if (user == null)
            {
                return null;
            }

            return ConvertUserToDTO(user);
        }

        public RoomDTO? GetRoomByUsers(string ownerId, string guestId) //TODO: Rename method
        {
            Room? room;

            room = _context.Rooms
                .FirstOrDefault(r => r.OwnerId == ownerId && r.GuestId == guestId);

            if (room == null)
            {
                return null;
            }

            return ConvertRoomToDTO(room);
        }

        public void SaveRoom(RoomDTO room)
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
            _context.SaveChanges();
        }

        public IEnumerable<RoomDTO> GetUserOwnRooms(string userId)
        {
            IEnumerable<Room> userOwnRooms = _context.Rooms
                .Where(r => r.OwnerId == userId)
                .Include(r => r.RoomSetting)
                .Include(r => r.Owner)
                .Include(r => r.Guest);

            return userOwnRooms.Select(ConvertRoomToDTO);
        }

        public IEnumerable<RoomDTO> GetUserGuestRooms(string userId)
        {
            IEnumerable<Room> userGuestRooms = _context.Rooms
                .Where(r => r.GuestId == userId)
                .Include(r => r.RoomSetting)
                .Include(r => r.Owner)
                .Include(r => r.Guest);

            return userGuestRooms.Select(ConvertRoomToDTO);
        }

        public IEnumerable<RoomDTO> GetRoomInvitations(string userId)
        {
            IEnumerable<Room> userRooms = _context.Rooms
                .Where(r => r.GuestId == userId && !r.InviteAccepted)
                .Include(r => r.RoomSetting)
                .Include(r => r.Owner)
                .Include(r => r.Guest);

            return userRooms.Select(ConvertRoomToDTO);
        }

        public RoomDTO? GetRoom(string id)
        {
            Room? room;

            room = _context.Rooms
                .Include(r => r.RoomSetting)
                .Include(r => r.Owner)
                .Include(r => r.Guest)
                .FirstOrDefault(r => r.Id == id);

            if (room == null)
            {
                return null;
            }

            return ConvertRoomToDTO(room);
        }

        public void DeleteRoom(string id)
        {
            Room? room = _context.Rooms.Find(id);

            if (room == null)
            {
                return;
            }

            _context.Rooms.Remove(room);
            _context.SaveChanges();
        }

        public IEnumerable<MovieDTO> GetUnscoredMovieInRoom(string roomId, string userId)
        {
            Room? room = _context.Rooms
                .Include(r => r.Movies)
                .FirstOrDefault(r => r.Id == userId);

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

        public IEnumerable<MovieDTO> GetMoviesInRoom(string roomId)
        {
            IEnumerable<RoomMovie> roomMovies = _context.RoomMovies
                .Where(rm => rm.RoomId == roomId)
                .Include(rm => rm.Movie);

            foreach (var item in roomMovies)
            {
                IEnumerable<MovieGenre> movieGenres = _context.MovieGenres
                .Include(mg => mg.Genre)
                .Where(mg => mg.MovieId == item.MovieId);

                item.Movie.Genres = movieGenres.ToList();
            }

            return roomMovies.Select(ConvertRoomMovieToDTO);
        }

        public RoomSettingsDTO? GetRoomSettingsById(string settingsId)
        {
            RoomSettings? roomSettings = _context.RoomSettings.Find(settingsId);

            if (roomSettings == null)
            {
                return null;
            }

            return ConvertRoomSettingsToDTO(roomSettings);
        }

        public void UpdateRoomSettings(RoomSettingsDTO roomSettings)
        {
            RoomSettings? settings = _context.RoomSettings.Find(roomSettings.Id);

            if (settings == null)
            {
                return;
            }

            settings.RoomId = roomSettings.RoomId;
            settings.MinKpRating = roomSettings.MinKpRaitings;
            settings.MaxKpRating = roomSettings.MaxKpRaitings;
            settings.MinYear = roomSettings.MinYear;
            settings.MaxYear = roomSettings.MaxYear;
            settings.TypeNumber = roomSettings.TypeNumber;

            _context.RoomSettings.Update(settings);
            _context.SaveChanges();
        }

        private Genre GetGenresRecordsByName(string name)
        {
            Genre? genre = _context.Genres
                .FirstOrDefault(x => x.Name == name);

            if (genre != null)
            {
                return genre;
            }

            genre = new Genre() 
            { 
                Name = name 
            };

            _context.Genres.Add(genre);
            _context.SaveChanges();
            return genre;
        }

        private RoomDTO ConvertRoomToDTO(Room room)
        {
            RoomDTO roomDTO = new RoomDTO
            {
                Id = room.Id,
                Owner = ConvertUserToDTO(room.Owner),
                Guest = ConvertUserToDTO(room.Guest),
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

        private MovieDTO ConvertMovieToDTO(Movie movie)
        {
            MovieDTO result = new MovieDTO
            {
                Id = movie.Id,
                Name = movie.Name,
                Description = movie.Description,
                Year = movie.Year,
                ImdbRaiting = movie.ImdbRating,
                KpRaiting = movie.KpRaiting,
                MovieLength = movie.MovieLength,
                TypeNumber = movie.TypeId,
                Poster = movie.Poster,
                Genres = movie.Genres.Select(ConvertGenreToDTO).ToList()
            };

            return result;
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

        private GenreDTO ConvertGenreToDTO(Genre genre)
        {
            GenreDTO result = new GenreDTO
            {
                Id = genre.Id,
                Name = genre.Name,
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
    }
}
