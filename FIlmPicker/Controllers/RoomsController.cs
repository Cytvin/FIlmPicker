using Microsoft.AspNetCore.Mvc;
using FIlmPicker.Models;
using System.Security.Claims;
using System.Text.Json;
using FIlmPicker.Services;
using FIlmPicker.Models.DTO;
using FIlmPicker.Converters;
using Microsoft.AspNetCore.Authorization;

namespace FIlmPicker.Controllers
{
    [Authorize]
    public class RoomsController : Controller
    {
        private readonly ILogger<RoomsController> _logger;
        private readonly DatabaseService _dbService;
        private readonly APIService _kinopoisk;

        public RoomsController(ILogger<RoomsController> logger, DatabaseService dbService, APIService kinopoisk)
        {
            _logger = logger;
            _dbService = dbService;
            _kinopoisk = kinopoisk;
        }

        public async Task<IActionResult> Index()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            RoomsViewModel viewModel = new RoomsViewModel();

            List<Room> userRooms = new List<Room>();

            IEnumerable<Room> ownerRooms = _dbService.GetUserOwnRooms(userId)
                .Where(r => r.OwnerIsOut == false)
                .Select(r => new Room(r));

            IEnumerable<Room> guestRooms = _dbService.GetUserGuestRooms(userId)
                .Where(r => r.InviteAccepted == true && r.GuestIsOut == false)
                .Select(r => new Room(r));

            userRooms.AddRange(ownerRooms);
            userRooms.AddRange(guestRooms);

            viewModel.Rooms = userRooms;
            viewModel.UnacceptedInviteCount = await _dbService.GetInviteCountAsync(userId);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(string guestLogin)
        {
            if (guestLogin == null)
            {
                TempData["GuestLoginError"] = $"Введите логин пользователя";
                return RedirectToAction("Index");
            }

            UserDTO? user = _dbService.GetUserByUserName(guestLogin);
            string ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (user == null)
            {
                TempData["GuestLoginError"] = $"Не найден пользователь с именем \"{guestLogin}\"";
                return RedirectToAction("Index");
            }

            User guest = new User(user);
            User owner = new User(_dbService.GetUserById(ownerId));

            if (guest.Id == ownerId)
            {
                TempData["GuestLoginError"] = "Вы не можете пригласить сами себя";
                return RedirectToAction("Index");
            }

            if (_dbService.GetRoomByUsers(ownerId, guest.Id) != null || _dbService.GetRoomByUsers(guest.Id, ownerId) != null)
            {
                TempData["GuestLoginError"] = $"У вас уже есть комната с пользователем\"{guestLogin}\"";
                return RedirectToAction("Index");
            }

            Room room = new Room(owner, guest);

            RoomSettings roomSettings = new RoomSettings(room.Id);
            room.SetRoomSettings(roomSettings);

            _dbService.SaveRoom(room.ToDTO());

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _logger.LogInformation("Get room {0} id ", id);

            if (id == null)
            {
                return BadRequest();
            }

            RoomDTO? roomDTO = await _dbService.GetRoom(id);

            if (roomDTO == null)
            {
                return NotFound();
            }

            Room room = new Room(roomDTO);

            if (!room.InviteAccepted)
            {
                ViewBag.Message = "Приглашение еще не принято";
                return View();
            }

            IEnumerable<Movie> unscoredMovies = _dbService.GetUnscoredMovieInRoom(room.Id, userId)
                .Select(m => new Movie(m));

            if (unscoredMovies.Count() > 0)
            {
                Movie unscoredMovie = unscoredMovies.First();

                _logger.Log(LogLevel.Information, $"MoviesInRoom: {unscoredMovie.Id}, {unscoredMovie.RoomId}, {unscoredMovie.GuestScore}, {unscoredMovie.OwnerScore}");

                RoomViewModel model = new RoomViewModel()
                {
                    Movie = unscoredMovie,
                    RoomId = room.Id,
                    SecondUserName = room.Owner.Id == userId ? room.Guest.UserName : room.Owner.UserName
                };

                return View(model);
            }

            Movie movie;

            List<Movie> movieInRoom = _dbService.GetMoviesInRoom(room.Id)
                .Select(m => new Movie(m)).ToList();

            room.RoomSettings.SetMovieInRoom(movieInRoom);

            try
            {
                MovieDTO movieDTO = await _kinopoisk.GetRandomMovieAsync(room.RoomSettings.GetQueryString());
                movieDTO.RoomId = room.Id;

                movie = new Movie(movieDTO);
            }
            catch (JsonException)
            {
                ViewBag.Message = "Ничего не найдено по фильтру";
                return View();
            }
            catch (InvalidOperationException)
            {
                ViewBag.Message = "Ничего не найдено по фильтру";
                return View();
            }
            catch (BadHttpRequestException)
            {
                return BadRequest();
            }

            _logger.Log(LogLevel.Information, $"{movie.Id}|{movie.Name}|{movie.Description}|{movie.TypeNumber}|{movie.MovieLength}");

            _dbService.SaveMovie(movie.ToDTO());
            _dbService.AddMovieToRoom(movie.Id, room.Id);

            RoomViewModel viewModel = new RoomViewModel()
            {
                Movie = movie,
                RoomId = room.Id,
                SecondUserName = room.Owner.UserName
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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

            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            RoomDTO? roomDTO = await _dbService.GetRoom(roomId);

            if (roomDTO == null)
            {
                return NotFound();
            }

            Room room = new Room(roomDTO);

            if (room.Owner.Id != userId && room.Guest.Id != userId)
            {
                _logger.Log(LogLevel.Information, $"User is not owner or guest in room {roomId}");
                return BadRequest();
            }

            int movieKpIdInt;

            if (!int.TryParse(movieKpId, out movieKpIdInt))
            {
                _logger.Log(LogLevel.Information, $"movieKpId can't be parsing to int {movieKpId}");
                return BadRequest();
            }

            MovieDTO? movieDTO = _dbService.GetMovieFromRoom(movieKpIdInt, room.Id);

            if (movieDTO == null)
            {
                return NotFound();
            }

            Movie movie = new Movie(movieDTO);

            UserScore userScore;

            if (!Enum.TryParse(score, out userScore))
            {
                _logger.Log(LogLevel.Information, $"score can't be parsing to UserScore Enum {userScore}");
                return BadRequest();
            }

            if (room.Owner.Id == userId)
            {
                movie.SetOwnerScore(userScore);
            }
            else
            {
                movie.SetGuestScore(userScore);
            }

            _dbService.SaveMovieScore(movie.ToDTO());

            return RedirectToAction("Details", new { id = roomId });
        }

        [HttpGet]
        public async Task<IActionResult> Matches(string id)
        {
            if (id == null)
            {
                _logger.Log(LogLevel.Information, $"RoomId is empty |{id}|");
                return BadRequest();
            }

            IEnumerable<Movie> moviesInRoom = _dbService.GetMoviesInRoom(id)
                .Select(m => new Movie(m));

            IEnumerable<Movie> movieMatches = moviesInRoom
                .Where(m => m.GuestScore == UserScore.Like && m.OwnerScore == UserScore.Like)
                .ToList();

            return PartialView("MatchesPartial", movieMatches);
        }

        [HttpGet]
        public async Task<IActionResult> Settings(string id)
        {
            if (id == null)
            {
                _logger.Log(LogLevel.Information, $"RoomId is empty");
                return BadRequest();
            }

            RoomDTO? roomDTO = await _dbService.GetRoom(id);

            if (roomDTO == null)
            {
                return BadRequest();
            }

            Room room = new Room(roomDTO);
            IEnumerable<Genre> genres = _dbService.GetAllGenres().Select(m => new Genre(m));

            RoomSettingsViewModel viewModel = new RoomSettingsViewModel
            {
                RoomSettings = room.RoomSettings,
                Genres = genres
            };

            return PartialView("SettingsPartial", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings(RoomSettingsBindingModel roomSettingsModel)
        {
            _logger.Log(LogLevel.Information, $"POST: {roomSettingsModel.Id} | {roomSettingsModel.MinKpRating} | {roomSettingsModel.MaxKpRating} | {roomSettingsModel.MinYear} | {roomSettingsModel.MaxYear} | {roomSettingsModel.TypeNumber}");

            if (!ModelState.IsValid)
            {
                _logger.Log(LogLevel.Information, $"Model not valid! ModelStateCount: {ModelState.Count} | ErrorCount: {ModelState.ErrorCount}");
                return BadRequest();
            }

            RoomSettingsDTO? roomSettingsDTO = _dbService.GetRoomSettingsById(roomSettingsModel.Id);

            if (roomSettingsDTO == null)
            {
                return BadRequest();
            }

            RoomSettings roomSettings = new RoomSettings(roomSettingsDTO);

            roomSettings.SetMinKpRating(roomSettingsModel.MinKpRating);
            roomSettings.SetMaxKpRating(roomSettingsModel.MaxKpRating);
            roomSettings.SetMinYear(roomSettingsModel.MinYear);
            roomSettings.SetMaxYear(roomSettingsModel.MaxYear);
            roomSettings.SetTypeNumber(roomSettingsModel.TypeNumber);

            roomSettings.RemoveAllGenre();
            if (roomSettingsModel.Genres != null)
            {
                foreach (string genreId in roomSettingsModel.Genres)
                {
                    GenreDTO? genreDTO = _dbService.GetGenre(genreId);

                    if (genreDTO == null)
                    {
                        continue;
                    }

                    Genre genre = new Genre(genreDTO);
                    roomSettings.AddGenre(genre);
                }
            }

            _dbService.UpdateRoomSettings(roomSettings.ToDTO());

            return RedirectToAction("Details", new { id = roomSettings.RoomId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Out(string id)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            RoomDTO? roomDTO = await _dbService.GetRoom(id);

            if (roomDTO == null)
            {
                return NotFound();
            }

            Room room = new Room(roomDTO);

            if (room.Owner.Id != userId && room.Guest.Id != userId)
            {
                return BadRequest();
            }

            if (room.Owner.Id == userId)
            {
                room.OwnerOut();
            }
            else
            {
                room.GuestOut();
            }

            if (room.GuestIsOut == true && room.OwnerIsOut == true)
            {
                await _dbService.DeleteRoomAsy(room.Id);
            }
            else
            {
                await _dbService.UpdateRoomAsync(room.ToDTO());
            }

            return RedirectToAction(nameof(Index));
        }
    }
}