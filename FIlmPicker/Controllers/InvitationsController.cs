using FIlmPicker.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FIlmPicker.Models;
using Microsoft.AspNetCore.Authorization;
using FIlmPicker.Converters;
using FIlmPicker.Models.DTO;

namespace FIlmPicker.Controllers
{
    [Authorize]
    public class InvitationsController : Controller
    {
        private readonly ILogger<InvitationsController> _logger;
        private readonly DatabaseService _dbService;
        private readonly MovieListUpdater _movieListUpdater;

        public InvitationsController(ILogger<InvitationsController> logger, DatabaseService dbService, MovieListUpdater movieListUpdater)
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

            IEnumerable<RoomDTO> roomsDTO = await _dbService.GetRoomInvitationsAsync(userId);
            List<Room> rooms = roomsDTO.Select(dto => new Room(dto))
                .ToList();

            return View(rooms);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(string id, string button)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (String.IsNullOrWhiteSpace(userId))
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            if (String.IsNullOrWhiteSpace(id) || String.IsNullOrWhiteSpace(button))
            {
                return BadRequest();
            }

            RoomDTO? roomDTO = await _dbService.GetRoomAsync(id);

            if (roomDTO == null)
            {
                return NotFound();
            }

            Room room = new Room(roomDTO);

            if (Guid.Equals(room.Id, userId))
            {
                return BadRequest();
            }

            if (room.InviteAccepted == true)
            {
                return RedirectToAction(nameof(Index));
            }

            if (String.Equals(button, "accept", StringComparison.OrdinalIgnoreCase))
            {
                room.AcceptInvite();
                await _dbService.UpdateRoomAsync(room.ToDTO());
                await _movieListUpdater.Update(room.RoomSettings);
            }
            else if (String.Equals(button, "reject", StringComparison.OrdinalIgnoreCase))
            {
                await _dbService.DeleteRoomAsync(id);
            }
            else
            {
                return BadRequest();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
