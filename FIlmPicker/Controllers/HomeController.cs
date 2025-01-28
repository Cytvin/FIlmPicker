using FIlmPicker.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using FIlmPicker.Services;

namespace FIlmPicker.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DatabaseService _dbService;
        private readonly BackgroundTaskQueue _queue;

        public HomeController(ILogger<HomeController> logger, DatabaseService db, BackgroundTaskQueue queue)
        {
            _logger = logger;
            _dbService = db;
            _queue = queue;
        }

        public async Task<IActionResult> Index()
        {
            await _queue.QueueBackgroundWorkItemAsync(token =>
            {
                _logger.LogInformation("Make some shit");
            });

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Rooms");
            }

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
