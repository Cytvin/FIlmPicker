using Microsoft.AspNetCore.Mvc;
using FIlmPicker.Models;
using System.Security.Claims;
using FIlmPicker.Services;
using FIlmPicker.Models.DTO;
using FIlmPicker.Converters;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;

namespace FIlmPicker.Controllers
{
    [Authorize]
    public class RoomsController : Controller
    {
        private readonly ILogger<RoomsController> _logger;
        private readonly DatabaseService _dbService;
        private readonly MovieListUpdater _movieListUpdater;

        public RoomsController(ILogger<RoomsController> logger, DatabaseService dbService, MovieListUpdater movieListUpdater)
        {
            _logger = logger;
            _dbService = dbService;
            _movieListUpdater = movieListUpdater;
        }

        public async Task<IActionResult> Index()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (String.IsNullOrWhiteSpace(userId))
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            RoomsViewModel viewModel = new RoomsViewModel();

            IEnumerable<RoomDTO> ownerRoomsDTO = await _dbService.GetUserOwnRoomsAsync(userId);
            IEnumerable<Room> ownerRooms = ownerRoomsDTO.Select(r => new Room(r));

            IEnumerable<RoomDTO> guestRoomsDTO = await _dbService.GetUserGuestRoomsAsync(userId);
            IEnumerable<Room> guestRooms = guestRoomsDTO.Select(r => new Room(r));

            viewModel.OwnerRooms = ownerRooms;
            viewModel.GuestRooms = guestRooms;

            viewModel.StatusMessage = TempData["StatusMessage"] != null ? JsonConvert.DeserializeObject<StatusMessage>(TempData["StatusMessage"].ToString()) : null;

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string guestLogin)
        {
            if (String.IsNullOrWhiteSpace(guestLogin))
            {
                StatusMessage errorMessage = new StatusMessage(StatusMessageType.Error, "Введите логин пользователя");
                TempData["StatusMessage"] = JsonConvert.SerializeObject(errorMessage);
                return RedirectToAction("Index");
            }

            string? ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (String.IsNullOrWhiteSpace(ownerId))
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            UserDTO? user = await _dbService.GetUserByUserNameAsync(guestLogin);

            if (user == null)
            {
                StatusMessage errorMessage = new StatusMessage(StatusMessageType.Error, $"Не найден пользователь с именем \"{guestLogin}\"");
                TempData["StatusMessage"] = JsonConvert.SerializeObject(errorMessage);
                return RedirectToAction("Index");
            }

            User guest = new User(user);

            UserDTO? ownerDTO = await _dbService.GetUserByIdAsync(ownerId);

            if (ownerDTO == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }    

            User owner = new User(ownerDTO);

            if (Guid.Equals(guest.Id, ownerId))
            {
                StatusMessage errorMessage = new StatusMessage(StatusMessageType.Error, "Вы не можете пригласить сами себя");
                TempData["StatusMessage"] = JsonConvert.SerializeObject(errorMessage);
                return RedirectToAction("Index");
            }

            RoomDTO? firstSearch = await _dbService.IsRoomWithUsersExistAsync(ownerId, guest.Id);
            RoomDTO? secondSearch = await _dbService.IsRoomWithUsersExistAsync(guest.Id, ownerId);

            if (firstSearch != null || secondSearch != null)
            {
                StatusMessage errorMessage = new StatusMessage(StatusMessageType.Error, $"У вас уже есть комната с пользователем \"{guestLogin}\"");
                TempData["StatusMessage"] = JsonConvert.SerializeObject(errorMessage);
                return RedirectToAction("Index");
            }

            Room room = new Room(owner, guest);

            RoomSettings roomSettings = new RoomSettings(room.Id);
            room.SetRoomSettings(roomSettings);

            await _dbService.SaveRoomAsync(room.ToDTO());

            StatusMessage successMessage = new StatusMessage(StatusMessageType.Success, $"Комната с пользователем \"{guestLogin}\" создана");
            TempData["StatusMessage"] = JsonConvert.SerializeObject(successMessage);

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

            MovieDTO? unscoredMovieDTO = await _dbService.GetUnscoredMovieInRoomAsync(room.Id, userId);

            if (unscoredMovieDTO != null)
            {
                Movie unscoredMovie = new Movie(unscoredMovieDTO);

                _logger.LogInformation("MoviesInRoom: {movieId}, {roomId}, {guestScore}, {OwnerScore}", unscoredMovie.Id, unscoredMovie.RoomId, unscoredMovie.GuestScore, unscoredMovie.OwnerScore);

                roomViewModel.Movie = unscoredMovie;
                roomViewModel.StatusOK = true;

                return View(roomViewModel);
            }

            await _movieListUpdater.Update(room.RoomSettings);

            return RedirectToAction(nameof(Index));
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

            MovieDTO? movieDTO = await _dbService.GetMovieFromRoomAsync(movieKpIdInt, room.Id);

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

            await _dbService.SaveMovieScoreAsync(movie.ToDTO());

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

            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (String.IsNullOrWhiteSpace(userId))
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            RoomDTO? roomDTO = await _dbService.GetRoomAsync(id);

            if (roomDTO == null)
            {
                _logger.LogInformation("Room with id '{id}' not found", id);
                return NotFound();
            }

            Room room = new Room(roomDTO);

            IEnumerable<MovieDTO> moviesInRoomDTO = await _dbService.GetMoviesInRoomAsync(id);
            IEnumerable<Movie> movies = moviesInRoomDTO.Select(m => new Movie(m));

            IEnumerable<Movie> matches = movies
                .Where(m => m.GuestScore == UserScore.Like && m.OwnerScore == UserScore.Like)
                .ToList();

            MatchesViewModel viewModel = new MatchesViewModel
            {
                Room = room,
                SecondUserName = room.Owner.Id == userId ? room.Guest.UserName : room.Owner.UserName,
                Movies = matches
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Out(string id)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (String.IsNullOrWhiteSpace(userId))
            {
                _logger.LogInformation("User Id is empty");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            RoomDTO? roomDTO = await _dbService.GetRoomAsync(id);

            if (roomDTO == null)
            {
                _logger.LogInformation("Room with id '{id}' not found", id);
                return NotFound();
            }

            Room room = new Room(roomDTO);

            if (Guid.Equals(room.Owner.Id, userId))
            {
                room.OwnerOut();
                _logger.LogInformation("User '{userId}' out from room '{roomId}' as owner", userId, room.Id);
            }
            else if (Guid.Equals(room.Guest.Id, userId))
            {
                room.GuestOut();
                _logger.LogInformation("User '{userId}' out from room '{roomId}' as guest", userId, room.Id);
            }
            else
            {
                _logger.LogInformation("User '{userId}' not in room '{roomId}'", userId, room.Id);
                return BadRequest();
            }

            if (room.GuestIsOut == true && room.OwnerIsOut == true || room.OwnerIsOut == true && room.InviteAccepted == false)
            {
                await _dbService.DeleteRoomAsync(room.Id);
                _logger.LogInformation("Room '{roomId}' deleted", room.Id);
            }
            else
            {
                await _dbService.UpdateRoomAsync(room.ToDTO());
                _logger.LogInformation("Room '{roomId}' updated", room.Id);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}