using FIlmPicker.Data;
using FIlmPicker.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using FIlmPicker.Data.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace FIlmPicker.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext dbContext)
        {
            _logger = logger;
            _context = dbContext;
        }

        public IActionResult Index()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                List<Room> userRooms = _context.Rooms
                    .Where(r => r.GuestId == currentUserId || r.OwnerId == currentUserId)
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

            IdentityUser? guest = _context.Users.FirstOrDefault(g => g.NormalizedUserName == guestLoginNormalized);
            string ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (guest == null)
            {
                TempData["GuestLoginError"] = $"Не найден пользователь с именем \"{guestLogin}\"";
            }
            else if (guest.Id == ownerId)
            {
                TempData["GuestLoginError"] = "Вы не можете пригласить сами себя";
            }
            else
            {
                if (_context.Rooms.Any(r => r.OwnerId == ownerId && r.GuestId == guest.Id))
                {
                    TempData["GuestLoginError"] = $"У вас уже есть комната с пользователем\"{guestLogin}\"";
                }
                else
                {
                    Room room = new Room();
                    room.OwnerId = ownerId;
                    room.GuestId = guest.Id;
                    room.InviteAccepted = false;

                    RoomSettings roomSettings = new RoomSettings
                    {
                        RoomId = room.Id,
                        MinKpRating = 1,
                        MaxKpRating = 10,
                        MinYear = 1990,
                        MaxYear = 2024,
                        TypeNumber = 1
                    };

                    room.RoomSetting = roomSettings;

                    _context.Rooms.Add(room);
                    _context.SaveChanges();
                }
            }

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
