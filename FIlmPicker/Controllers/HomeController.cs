using FIlmPicker.Data;
using FIlmPicker.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using FIlmPicker.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Identity.Core;
using System.Security.Claims;

namespace FIlmPicker.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _dbContext;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                List<Room> userRooms = _dbContext.Rooms
                    .Where(r => r.GuestId == currentUserId || r.OwnerId == currentUserId)
                    .ToList();

                ViewBag.Id = currentUserId;

                return View(userRooms);
            }

            return View();
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
