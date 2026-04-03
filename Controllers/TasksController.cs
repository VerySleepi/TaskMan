using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TaskMan.Data;
using TaskMan.Models;
using TaskMan.ViewModels.Tasks;

namespace TaskMan.Controllers
{
    public class TasksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> userManager;

        public TasksController(ApplicationDbContext context, UserManager<User> userManager)
        {
            this.userManager = userManager;
            _context = context;
        }

        // GET: TasksController
        public async Task<IActionResult> Dashboard()
        {
            ViewData["userName"] = userManager.GetUserName(User);
            var userId = userManager.GetUserId(HttpContext.User);
            var userTasksList = await _context.UserTasks
                .Where(t => t.User_ID == userId)
                .ToListAsync();

            var userListIds = userTasksList.Select(ut => ut.User_List_ID).ToList();

            var tasksList = await _context.Task
                .Where(t => userListIds.Contains(t.User_List_ID))
                .ToListAsync();

            var model = new DashboardViewModel
            {
                structuredUserTasks = userTasksList.Select(ut => new UserTasksViewModel
                {
                    UserTask = ut,
                    Tasks = tasksList.Where(t => t.User_List_ID == ut.User_List_ID).ToList()
                }).ToList()
            };
            return View(model);
        }

        // GET: TasksController/Details/5
        public ActionResult Details(int taskid)
        {
            return View();
        }

        // GET: TasksController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: TasksController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
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

        // GET: TasksController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: TasksController/Edit/5
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

        // GET: TasksController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: TasksController/Delete/5
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
