using FIlmPicker.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using FIlmPicker.Services;
using FIlmPicker.Models.DTO;
using FIlmPicker.Converters;

namespace FIlmPicker.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DatabaseService _dbService;

        public HomeController(ILogger<HomeController> logger, DatabaseService db)
        {
            _logger = logger;
            _dbService = db;
        }

        public IActionResult Index()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                List<Room> userRooms = _dbService.GetUserRoomsById(currentUserId)
                    .Select(s => new Room(s))
                    .ToList();

                ViewBag.Id = currentUserId;

                return View(userRooms);
            }

            return View();
        }

        [HttpPost]
        public IActionResult CreateRoom(string guestLogin)
        {
            if (User.Identity != null && !User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            string guestLoginNormalized = guestLogin.Trim().ToUpper();

            UserDTO? user = _dbService.GetUserByUserName(guestLoginNormalized);
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

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
