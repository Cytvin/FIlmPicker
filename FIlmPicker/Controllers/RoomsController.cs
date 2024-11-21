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

            if (String.IsNullOrWhiteSpace(userId))
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            RoomsViewModel viewModel = new RoomsViewModel();

            List<Room> userRooms = new List<Room>();

            IEnumerable<RoomDTO> ownerRoomsDTO = await _dbService.GetUserOwnRooms(userId);
            IEnumerable<Room> ownerRooms = ownerRoomsDTO.Select(r => new Room(r));

            IEnumerable<RoomDTO> guestRoomsDTO = await _dbService.GetUserGuestRooms(userId);
            IEnumerable<Room> guestRooms = guestRoomsDTO.Select(r => new Room(r));

            userRooms.AddRange(ownerRooms);
            userRooms.AddRange(guestRooms);

            viewModel.Rooms = userRooms;
            viewModel.UnacceptedInviteCount = await _dbService.GetInviteCountAsync(userId);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string guestLogin)
        {
            if (String.IsNullOrWhiteSpace(guestLogin))
            {
                TempData["GuestLoginError"] = $"Введите логин пользователя";
                return RedirectToAction("Index");
            }

            string? ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (String.IsNullOrWhiteSpace(ownerId))
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            UserDTO? user = await _dbService.GetUserByUserName(guestLogin);

            if (user == null)
            {
                TempData["GuestLoginError"] = $"Не найден пользователь с именем \"{guestLogin}\"";
                return RedirectToAction("Index");
            }

            User guest = new User(user);
            User owner = new User(_dbService.GetUserById(ownerId));

            if (Guid.Equals(guest.Id, ownerId))
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

            if (String.IsNullOrWhiteSpace(userId))
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            _logger.LogInformation("Get room {roomId}", id);

            RoomDTO? roomDTO = await _dbService.GetRoomAsync(id);

            if (roomDTO == null)
            {
                _logger.LogInformation("Room with id '{roomId}' not found", id);
                return NotFound();
            }

            Room room = new Room(roomDTO);
            RoomViewModel roomViewModel = new RoomViewModel()
            {
                RoomId = room.Id,
                SecondUserName = room.Owner.Id == userId ? room.Guest.UserName : room.Owner.UserName
            };

            if (!room.InviteAccepted)
            {
                roomViewModel.StatusMessage = "Приглашение еще не принято";
                _logger.LogInformation("Invite not accepted in room {id}", room.Id);
                return View(roomViewModel);
            }

            IEnumerable<Movie> unscoredMovies = _dbService.GetUnscoredMovieInRoom(room.Id, userId)
                .Select(m => new Movie(m));

            if (unscoredMovies.Count() > 0)
            {
                Movie unscoredMovie = unscoredMovies.First();

                _logger.LogInformation("MoviesInRoom: {movieId}, {roomId}, {guestScore}, {OwnerScore}", unscoredMovie.Id, unscoredMovie.RoomId, unscoredMovie.GuestScore, unscoredMovie.OwnerScore);

                roomViewModel.Movie = unscoredMovie;
                roomViewModel.StatusOK = true;

                return View(roomViewModel);
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
                roomViewModel.StatusMessage = "Ничего не найдено по фильтру";
                return View(roomViewModel);
            }
            catch (InvalidOperationException)
            {
                roomViewModel.StatusMessage = "Ничего не найдено по фильтру";
                return View(roomViewModel);
            }
            catch (BadHttpRequestException)
            {
                return BadRequest();
            }

            _logger.LogInformation("Movie from API: {Id}; {Name}; {Description}; {TypeNumber}; {MovieLength}",
                movie.Id, movie.Name, movie.Description, movie.TypeNumber, movie.MovieLength);

            _dbService.SaveMovie(movie.ToDTO());
            _dbService.AddMovieToRoom(movie.Id, room.Id);

            roomViewModel.Movie = movie;
            roomViewModel.StatusOK = true;

            return View(roomViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ScoreMovie(string roomId, string movieKpId, string score)
        {
            _logger.LogInformation("Get Movie Data and score: {roomId}; {movieKpId}; {score}", roomId, movieKpId, score);

            if (String.IsNullOrWhiteSpace(roomId) || String.IsNullOrWhiteSpace(movieKpId) || String.IsNullOrWhiteSpace(score))
            {
                _logger.LogInformation("Some parameter is empty");
                return BadRequest();
            }

            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (String.IsNullOrWhiteSpace(userId))
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            RoomDTO? roomDTO = await _dbService.GetRoomAsync(roomId);

            if (roomDTO == null)
            {
                return NotFound();
            }

            Room room = new Room(roomDTO);

            int movieKpIdInt;

            if (!int.TryParse(movieKpId, out movieKpIdInt))
            {
                _logger.LogInformation("movieKpId can't be parsing to int {movieKpId}", movieKpId);
                return BadRequest();
            }

            MovieDTO? movieDTO = await _dbService.GetMovieFromRoom(movieKpIdInt, room.Id);

            if (movieDTO == null)
            {
                _logger.LogInformation("Movie {movieKpIdInt} not found in room {roomId}", movieKpIdInt, room.Id);
                return NotFound();
            }

            Movie movie = new Movie(movieDTO);

            UserScore userScore;

            if (!Enum.TryParse(score, out userScore))
            {
                _logger.LogInformation("score can't be parsing to UserScore Enum {userScore}", userScore);
                return BadRequest();
            }

            if (Guid.Equals(room.Owner.Id, userId))
            {
                movie.SetOwnerScore(userScore);
            }
            else if (Guid.Equals(room.Guest.Id, userId))
            {
                movie.SetGuestScore(userScore);
            }
            else
            {
                _logger.LogInformation("User is not owner or guest in room {roomId}", roomId);
                return BadRequest();
            }

            await _dbService.SaveMovieScore(movie.ToDTO());

            _logger.LogInformation("Movie score saved");

            return RedirectToAction("Details", new { id = roomId });
        }

        [HttpGet]
        public async Task<IActionResult> Matches(string id)
        {
            if (String.IsNullOrWhiteSpace(id))
            {
                _logger.LogInformation("RoomId is empty {id}", id);
                return BadRequest();
            }

            IEnumerable<Movie> moviesInRoom = _dbService.GetMoviesInRoom(id)
                .Select(m => new Movie(m));

            IEnumerable<Movie> movieMatches = moviesInRoom
                .Where(m => m.GuestScore == UserScore.Like && m.OwnerScore == UserScore.Like)
                .ToList();

            return View(movieMatches);
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

            RoomDTO? roomDTO = await _dbService.GetRoomAsync(id);

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
                await _dbService.DeleteRoomAsync(room.Id);
            }
            else
            {
                await _dbService.UpdateRoomAsync(room.ToDTO());
            }

            return RedirectToAction(nameof(Index));
        }
    }
}