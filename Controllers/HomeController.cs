using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TaskMan.Models;
using TaskMan.Services;
using TaskMan.ViewModels;

namespace TaskMan.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, UserManager<User> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(HttpContext.User);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                Debug.WriteLine($"User with ID {userId} not found.");
                return RedirectToAction("Login", "Account");
            }
            Debug.WriteLine($"User ID: {userId}, User Name: {user.UserName}, Profile Pic: {user.Profile_Pic}");
            ViewData["userName"] = user.UserName;
            ViewData["profilePic"] =  Url.Content($"~/images/{user.Profile_Pic}");


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
