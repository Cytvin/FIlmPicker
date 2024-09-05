using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FIlmPicker.Data;
using FIlmPicker.Data.Models;
using FIlmPicker.Models;
using System.Security.Claims;
using FIlmPicker.KinopoiskAPI;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;
using System.Collections.Immutable;

namespace FIlmPicker.Controllers
{
    public class RoomsController : Controller
    {
        private readonly ILogger<RoomsController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly KinopoiskAPIFacade _kinopoisk;

        public RoomsController(ILogger<RoomsController> logger, ApplicationDbContext context, KinopoiskAPIFacade kinopoisk)
        {
            _logger = logger;
            _context = context;
            _kinopoisk = kinopoisk;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            RoomsViewModel viewModel = new RoomsViewModel();

            viewModel.OwnerRooms = _context.Rooms
                .Where(r => r.OwnerId == userId)
                .Include(r => r.Owner)
                .Include(r => r.Guest)
                .ToList();

            viewModel.GuestRooms = _context.Rooms
                .Where(r => r.GuestId == userId && r.InviteAccepted)
                .Include(r => r.Owner)
                .Include(r => r.Guest)
                .ToList();

            viewModel.UnacceptedInviteCount = _context.Rooms
                .Count(r => r.GuestId == userId && !r.InviteAccepted);

            return View(viewModel);
        }

        public async Task<IActionResult> Invitations()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            List<Room> rooms = await _context.Rooms
                .Where(r => r.GuestId == userId && !r.InviteAccepted)
                .Include(r => r.Owner)
                .Include(r => r.Guest)
                .ToListAsync();

            return PartialView("InvitationsPartial", rooms);
        }

        public async Task<IActionResult> UpdateInvite(string id, string button)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            if (id == null)
            {
                return BadRequest();
            }

            Room? room = await _context.Rooms.FindAsync(id);

            if (room == null)
            {
                return NotFound();
            }

            if (button == "accept")
            {
                room.InviteAccepted = true;
                _context.Update(room);
            }
            else if (button == "reject")
            {
                _context.Remove(room);
            }
            else
            {
                return BadRequest();
            }

            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (id == null)
            {
                return BadRequest();
            }

            Room? room = await _context.Rooms
                .Include(r => r.Movies)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (room == null)
            {
                return NotFound();
            }

            if (!room.InviteAccepted)
            {
                ViewBag.Message = "Приглашение еще не принято";
                return PartialView("RoomPartial");
            }

            IdentityUser? roomOwner = await _context.Users.FindAsync(room.OwnerId);

            IEnumerable<RoomMovie> unscoredMovies;

            if (room.OwnerId == userId)
            {
                unscoredMovies = room.Movies.Where(m => m.OwnerScore == (int)UserScore.None);
            }
            else
            {
                unscoredMovies = room.Movies.Where(m => m.GuestScore == (int)UserScore.None);
            }

            MovieAPIModel movie;

            if (unscoredMovies.Count() > 0)
            {
                RoomMovie unscoredMovie = unscoredMovies.First();

                _logger.Log(LogLevel.Information, $"MoviesInRoom: {unscoredMovie.RoomId}, {unscoredMovie.MovieId}, {unscoredMovie.GuestScore}, {unscoredMovie.OwnerScore}");

                movie = await _kinopoisk.GetMovieById(unscoredMovie.MovieId);
            }
            else
            {
                try
                {
                    movie = await _kinopoisk.GetRandomMovie();
                }
                catch (JsonException ex)
                {
                    ViewBag.Message = "Ничего не найдено по фильтру";
                    return PartialView("RoomPartial");
                }
                catch (BadHttpRequestException ex)
                {
                    return BadRequest();
                }

                _logger.Log(LogLevel.Information, $"{movie.Id}|{movie.Name}|{movie.Description}|{movie.TypeNumber}|{movie.MovieLength}" +
                    $"|{movie.SeriesLength}|{movie.Year}|{movie.AlternativeName}|");

                List<Genre> genres = new List<Genre>();

                foreach(GenreAPIModel genreAPI in movie.Genres)
                {
                    Genre? genre = _context.Genres.FirstOrDefault(g => g.Name == genreAPI.Name);

                    if (genre == null)
                    {
                        genre = new Genre();
                        genre.Name = genreAPI.Name;
                        _context.Genres.Add(genre);
                        _context.SaveChanges();
                    }

                    genres.Add(genre);
                }

                Movie movieRecord = new Movie();
                movieRecord.Id = movie.Id;
                movieRecord.Name = movie.Name ?? movie.AlternativeName;
                movieRecord.Description = movie.Description;
                movieRecord.TypeId = movie.TypeNumber;
                movieRecord.MovieLength = movie.MovieLength ?? movie.SeriesLength ?? 0;
                movieRecord.Year = movie.Year ?? 0;
                movieRecord.KpRaiting = movie.Rating.Kp;
                movieRecord.ImdbRating = movie.Rating.Imdb;
                movieRecord.Poster = movie.Poster.Url;
                movieRecord.Genres = genres.Select<Genre, MovieGenre>(g => new MovieGenre() { MovieId = movie.Id, GenreId = g.Id }).ToList();

                RoomMovie newMovie = new RoomMovie();
                newMovie.RoomId = room.Id;
                newMovie.MovieId = movie.Id;
                newMovie.GuestScore = (int)UserScore.None;
                newMovie.GuestScore = (int)UserScore.None;

                _logger.Log(LogLevel.Information, $"MoviesInRoom: {newMovie.RoomId}, {newMovie.MovieId}, {newMovie.GuestScore}, {newMovie.OwnerScore}");

                _context.Movies.Add(movieRecord);
                _context.RoomMovies.Add(newMovie);
                _context.SaveChanges();
            }

            RoomViewModel viewModel = new RoomViewModel();
            viewModel.Movie = movie;
            viewModel.RoomId = room.Id;
            viewModel.OwnerUserName = roomOwner.UserName;

            return PartialView("RoomPartial", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ScoreMovie(string roomId, string movieKpId, string score)
        {
            _logger.Log(LogLevel.Information, $"Get 'POST' Query with data:" +
                $"\n\troomId: {roomId}," +
                $"\n\tmovieKpId: {movieKpId}," +
                $"\n\tscore: {score}.");

            if (roomId == null || movieKpId == null || score == null)
            {
                _logger.Log(LogLevel.Information, $"Some parameter is empty");
                return BadRequest();
            }

            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Room? room = await _context.Rooms
                .Include(r => r.Movies)
                .FirstOrDefaultAsync(r => r.Id == roomId);

            if (room == null)
            {
                return NotFound();
            }

            if (room.OwnerId != userId && room.GuestId != userId)
            {
                _logger.Log(LogLevel.Information, $"User not owner and guest in room {roomId}");
                return BadRequest();
            }

            int movieKpIdInt;

            if (!int.TryParse(movieKpId, out movieKpIdInt))
            {
                _logger.Log(LogLevel.Information, $"movieKpId can't be parsing to int {movieKpId}");
                return BadRequest();
            }

            RoomMovie? movie = room.Movies.FirstOrDefault(m => m.MovieId == movieKpIdInt);

            if (movie == null)
            {
                return NotFound();
            }

            UserScore userScore;

            if (!Enum.TryParse(score, out userScore))
            {
                _logger.Log(LogLevel.Information, $"score can't be parsing to UserScore Enum {userScore}");
                return BadRequest();
            }

            if (room.OwnerId == userId)
            {
                movie.OwnerScore = (int)userScore;
            }
            else
            {
                movie.GuestScore = (int)userScore;
            }

            _context.Update(movie);
            _context.SaveChanges();

            return RedirectToAction("Details", new { id = roomId });
        }

        [HttpGet]
        public async Task<IActionResult> Matches(string id)
        {
            if (id == null)
            {
                _logger.Log(LogLevel.Information, $"RoomId is empty");
                return BadRequest();
            }

            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            Room? room = await _context.Rooms
                .Include(r => r.Movies)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (room == null)
            {
                return NotFound();
            }

            IEnumerable<RoomMovie> movieMatches = room.Movies
                .Where(m => m.GuestScore == (int)UserScore.Like && m.OwnerScore == (int)UserScore.Like);

            List<Movie> movieList = movieMatches.Select<RoomMovie, Movie>(m => _context.Movies.Find(m.MovieId)).ToList();

            return PartialView("MatchesPartial", movieList);
        }

        [HttpGet]
        public async Task<IActionResult> Settings(string id)
        {
            if (id == null)
            {
                _logger.Log(LogLevel.Information, $"RoomId is empty");
                return BadRequest();
            }

            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            Room? room = await _context.Rooms.Include(r => r.RoomSetting).FirstOrDefaultAsync(r => r.Id == id);

            if (room == null)
            {
                return BadRequest();
            }

            return PartialView("SettingsPartial", room.RoomSetting);
        }

        [HttpPost]
        public async Task<IActionResult> Settings(RoomSettingsBindingModel roomSettingsModel)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            _logger.Log(LogLevel.Information, $"POST: {roomSettingsModel.Id} | {roomSettingsModel.MinKpRating} | {roomSettingsModel.MaxKpRating} | {roomSettingsModel.MinYear} | {roomSettingsModel.MaxYear} | {roomSettingsModel.TypeNumber}");

            if (!ModelState.IsValid)
            {
                _logger.Log(LogLevel.Information, $"Model not valid! ModelStateCount: {ModelState.Count} | ErrorCount: {ModelState.ErrorCount}");
                return BadRequest();
            }

            RoomSettings? roomSettings = await _context.RoomSettings.FindAsync(roomSettingsModel.Id);

            if (roomSettings == null)
            {
                return BadRequest();
            }

            roomSettings.MinKpRating = roomSettingsModel.MinKpRating;
            roomSettings.MaxKpRating = roomSettingsModel.MaxKpRating;
            roomSettings.MinYear = roomSettingsModel.MinYear;
            roomSettings.MaxYear = roomSettingsModel.MaxYear;
            roomSettings.TypeNumber = roomSettingsModel.TypeNumber;

            _context.Update(roomSettings);
            _context.SaveChanges();

            return RedirectToAction("Details", new { id = roomSettings.RoomId });
        }
    }
}