using FIlmPicker.Models.DTO;
using FIlmPicker.Models;
using Microsoft.AspNetCore.Mvc;
using FIlmPicker.Services;
using FIlmPicker.Converters;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using FIlmPicker.Services.DatabaseServices;

namespace FIlmPicker.Controllers
{
    [Authorize]
    public class RoomSettingsController : Controller
    {
        private readonly ILogger<RoomsController> _logger;
        private readonly DatabaseService _dbService;
        private readonly MovieListUpdater _movieListUpdater;

        public RoomSettingsController(ILogger<RoomsController> logger, DatabaseService dbService, MovieListUpdater movieListUpdater)
        {
            _logger = logger;
            _dbService = dbService;
            _movieListUpdater = movieListUpdater;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string id)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (String.IsNullOrWhiteSpace(userId)) 
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            if (String.IsNullOrEmpty(id))
            {
                _logger.LogInformation("RoomId is null");
                return BadRequest();
            }

            RoomDTO? roomDTO = await _dbService.GetRoomAsync(id);

            if (roomDTO == null)
            {
                _logger.LogInformation("Room with id {roomId} not found", id);
                return BadRequest();
            }

            Room room = new Room(roomDTO);

            if (!Guid.Equals(room.Owner.Id, userId) && !Guid.Equals(room.Guest.Id, userId))
            {
                _logger.LogInformation("User {userId} not in room {roomId}", userId, room.Id);
                return BadRequest();
            }

            IEnumerable<GenreDTO> genresDTO = await _dbService.GetAllGenresAsync();
            IEnumerable<Genre> genres = genresDTO.Select(g => new Genre(g));

            RoomSettingsViewModel viewModel = new RoomSettingsViewModel
            {
                RoomId = room.Id,
                RoomSettings = room.RoomSettings,
                SecondUserName = room.Owner.Id == userId ? room.Guest.UserName : room.Owner.UserName,
                StatusMessage = TempData["StatusMessage"] != null ? JsonConvert.DeserializeObject<StatusMessage>(TempData["StatusMessage"].ToString()) : null,
                Genres = genres
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(RoomSettingsBindingModel roomSettingsModel)
        {
            _logger.LogInformation("Update: ID: {Id}; MinKpRaiting: {MinKpRating}; MaxKpRating: {MaxKpRating}; MinYear: {MinYear}; MaxYear: {MaxYear}; TypeNumber: {TypeNumber};",
                roomSettingsModel.Id, roomSettingsModel.MinKpRating, roomSettingsModel.MaxKpRating, roomSettingsModel.MinYear, roomSettingsModel.MaxYear, roomSettingsModel.TypeNumber);

            if (!ModelState.IsValid)
            {
                _logger.LogInformation("Model not valid. ModelStateCount: {Count}; ErrorCount: {ErrorCount}", ModelState.Count, ModelState.ErrorCount);
                return BadRequest();
            }

            RoomSettingsDTO? roomSettingsDTO = await _dbService.GetRoomSettingsByIdAsync(roomSettingsModel.Id);

            if (roomSettingsDTO == null)
            {
                _logger.LogInformation("RoomSettings with id {id} not found", roomSettingsModel.Id);
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
                    GenreDTO? genreDTO = await _dbService.GetGenreByIdAsync(genreId);

                    if (genreDTO == null)
                    {
                        continue;
                    }

                    Genre genre = new Genre(genreDTO);
                    roomSettings.AddGenre(genre);
                }
            }

            await _dbService.UpdateRoomSettingsAsync(roomSettings.ToDTO());
            _logger.LogInformation("Room settings updated");
            await _dbService.RemoveUnscoredMovieFromRoomAsync(roomSettings.RoomId);
            _logger.LogInformation("Unscored movie deleted");
            await _dbService.CreateMovieListOnUpdate(roomSettings.RoomId);
            await _movieListUpdater.Update(roomSettings);

            StatusMessage successMessage = new StatusMessage(StatusMessageType.Success, "Настройки сохранены");
            TempData["StatusMessage"] = JsonConvert.SerializeObject(successMessage);

            return RedirectToAction("Index", new { id = roomSettings.RoomId });
        }
    }
}
