using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TaskMan.Models;
using TaskMan.Services;

namespace TaskMan.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ILogger<HomeController> _logger;
        private readonly IEmailSender _emailSender;

        public HomeController(ILogger<HomeController> logger, IEmailSender emailSender, UserManager<User> userManager)
        {
            _logger = logger;
            _emailSender = emailSender;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.userName = _userManager.GetUserName(HttpContext.User);

            EmailSender email = new EmailSender();
            //var recieverEmail = "George.O.Stead@student.shu.ac.uk";
            //var subject = "New Login Detected";
            //var message = $"A new login to your account was detected at {DateTime.Now}. If this was you, you can safely ignore this email. If this wasn't you, we recommend changing your password immediately.";

            //await _emailSender.SendEmailAsync(recieverEmail, subject, message);
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
