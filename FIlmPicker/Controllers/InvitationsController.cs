using FIlmPicker.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FIlmPicker.Models;
using Microsoft.AspNetCore.Authorization;
using FIlmPicker.Services.DatabaseServices;

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

            IEnumerable<Room> rooms = await _dbService.RoomService.GetInvitationsAsync(userId);

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

            Room? room = await _dbService.RoomService.GetRoomAsync(id);

            if (room == null)
            {
                return NotFound();
            }

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
                await _dbService.RoomService.UpdateRoomAsync(room);
                await _movieListUpdater.Update(room.RoomSettings);
            }
            else if (String.Equals(button, "reject", StringComparison.OrdinalIgnoreCase))
            {
                await _dbService.RoomService.DeleteRoomAsync(id);
            }
            else
            {
                return BadRequest();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
