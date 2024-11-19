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
        private readonly APIService _kinopoisk;

        public InvitationsController(ILogger<InvitationsController> logger, DatabaseService dbService, APIService kinopoisk)
        {
            _logger = logger;
            _dbService = dbService;
            _kinopoisk = kinopoisk;
        }

        public async Task<IActionResult> Index()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
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

            if (room.InviteAccepted == true)
            {
                return RedirectToAction(nameof(Index));
            }

            if (button == "accept")
            {
                room.AcceptInvite();
                await _dbService.UpdateRoomAsync(room.ToDTO());
            }
            else if (button == "reject")
            {
                await _dbService.DeleteRoomAsy(id);
            }
            else
            {
                return BadRequest();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
