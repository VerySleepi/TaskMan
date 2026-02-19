using Microsoft.AspNetCore.Mvc;

namespace TaskMan.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
