using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TaskMan.Data;
using TaskMan.Models;
using TaskMan.ViewModels.Group;

namespace TaskMan.Controllers
{
    [Authorize]
    public class GroupsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public GroupsController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: GroupsController
        public ActionResult Index()
        {
            return View();
        }

        // GET: GroupsController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: GroupsController/Create
        [HttpGet]
        public IActionResult Create()
        {
            var model = new NewGroupViewModel();
            return PartialView("_CreateGroup", model);
        }

        // POST: GroupsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NewGroupViewModel model)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var group = new Group
                {
                    Title = model.Title,
                    Description = model.Description
                };

                _context.Group.Add(group);
                await _context.SaveChangesAsync();

                var groupAdmin = new GroupUsers
                {
                    User_ID = user.Id,
                    Group_ID = group.Group_ID,
                    Role = "Admin",
                    Join_Date = DateOnly.FromDateTime(DateTime.Today)
                };

                _context.GroupUsers.Add(groupAdmin);
                await _context.SaveChangesAsync();

                // Temporary redirect to tasks dashboard, should be to group dashboard once implemented
                return RedirectToAction("Dashboard", "Tasks");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error creating group: " + ex.Message);
                return RedirectToAction("Dashboard", "Tasks");
            }
        }

        // GET: GroupsController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: GroupsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: GroupsController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: GroupsController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
