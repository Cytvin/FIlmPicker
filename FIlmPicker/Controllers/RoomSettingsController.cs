using FIlmPicker.Models.DTO;
using FIlmPicker.Models;
using Microsoft.AspNetCore.Mvc;
using FIlmPicker.Services;
using FIlmPicker.Converters;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace FIlmPicker.Controllers
{
    [Authorize]
    public class RoomSettingsController : Controller
    {
        private readonly ILogger<RoomsController> _logger;
        private readonly DatabaseService _dbService;
        private readonly APIService _kinopoisk;

        public RoomSettingsController(ILogger<RoomsController> logger, DatabaseService dbService, APIService kinopoisk)
        {
            _logger = logger;
            _dbService = dbService;
            _kinopoisk = kinopoisk;
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

            IEnumerable<GenreDTO> genresDTO = await _dbService.GetAllGenres();
            IEnumerable<Genre> genres = genresDTO.Select(g => new Genre(g));

            RoomSettingsViewModel viewModel = new RoomSettingsViewModel
            {
                RoomId = room.Id,
                RoomSettings = room.RoomSettings,
                SecondUserName = room.Owner.Id == userId ? room.Guest.UserName : room.Owner.UserName,
                StatusMessage = TempData["StatusMessage"]?.ToString() ?? null,
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
                    GenreDTO? genreDTO = await _dbService.GetGenreAsync(genreId);

                    if (genreDTO == null)
                    {
                        continue;
                    }

                    Genre genre = new Genre(genreDTO);
                    roomSettings.AddGenre(genre);
                }
            }

            await _dbService.UpdateRoomSettings(roomSettings.ToDTO());

            TempData["StatusMessage"] = "Настройки сохранены";

            return RedirectToAction("Index", new { id = roomSettings.RoomId });
        }
    }
}
