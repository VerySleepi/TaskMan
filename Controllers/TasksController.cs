using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TaskMan.Data;
using TaskMan.Models;
using TaskMan.ViewModels.Tasks;

namespace TaskMan.Controllers
{
    [Authorize]
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
            var userId = userManager.GetUserId(HttpContext.User);
            var user = await userManager.FindByIdAsync(userId);
            // Pass user information to the view using ViewData
            ViewData["userName"] = user.UserName;
            ViewData["profilePic"] = Url.Content($"~/images/{user.Profile_Pic}");


            // Fetch UserTasks for the current user
            var userTasksList = await _context.UserTasks
                .Where(t => t.User_ID == userId)
                .ToListAsync();

            userTasksList.Reverse();

            // Extract User_List_IDs from the fetched UserTasks
            var userListIds = userTasksList.Select(ut => ut.User_List_ID).ToList();


            // Fetch Tasks that match the extracted User_List_IDs
            var tasksList = await _context.Task
                .Where(t => userListIds.Contains(t.User_List_ID))
                .ToListAsync();

            // Structure the data into the DashboardViewModel
            var model = new DashboardViewModel
            {
                // For each UserTasks, find the corresponding Tasks and create a UserTasksViewModel
                // and add it to the structuredUserTasks list
                structuredUserTasks = userTasksList.Select(ut => new UserTasksViewModel
                {
                    UserTask = ut,
                    Tasks = tasksList.Where(t => t.User_List_ID == ut.User_List_ID).ToList()
                }).ToList(),
            };
            return View(model);
        }

        // GET: TasksController/Details/5
        public ActionResult Details()
        {
            return View();
        }

        // GET: TasksController/CreateTask
        [HttpGet]
        public async Task<IActionResult> CreateTask()
        {
            var userId = userManager.GetUserId(HttpContext.User);

            var userTaskLists = await _context.UserTasks
                .Where(ut => ut.User_ID == userId && ut.Title != "Completed Tasks")
                .ToListAsync();

            userTaskLists.Reverse();

            var model = new NewTaskViewModel
            {
                Due_Date = DateOnly.FromDateTime(DateTime.Now),
                Status = 0,
                UserTaskLists = userTaskLists
            };

            return PartialView("_CreateTask", model);
        }

        // POST: TasksController/CreateTask
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTask(NewTaskViewModel model)
        {
            // Fetch Tasks that match the extracted User_List_IDs
            var tasksList = await _context.Task
                .Where(t => t.User_List_ID == model.User_List_ID && t.Title.StartsWith("New Task"))
                .ToListAsync();
            try
            {
                if (model.Title == null || model.Title == string.Empty)
                {
                    if (tasksList.Count > 0)
                    {
                        model.Title = $"New Task ({tasksList.Count})";

                    }
                    else
                    {
                        model.Title = "New Task";

                    }
                }
                if (model.Description == null)
                {
                    model.Description = "No Description";
                }

                if (!ModelState.IsValid)
                {
                    Debug.WriteLine(model.Title);
                    Debug.WriteLine(model.Description);
                    foreach (var entry in ModelState)
                    {
                        Debug.WriteLine($"{entry.Key}: {string.Join(", ", entry.Value.Errors.Select(e => e.ErrorMessage))}");
                    }
                }

                var task = new TaskMan.Models.Task
                {
                    Title = model.Title,
                    Description = model.Description,
                    Due_Date = model.Due_Date,
                    Status = 0,
                    User_List_ID = model.User_List_ID
                };

                _context.Task.Add(task);
                await _context.SaveChangesAsync();

                return RedirectToAction("Dashboard");
            }
            catch
            {
                return RedirectToAction("Dashboard");
            }
        }


        // GET: TasksController/CreateList
        [HttpGet]
        public async Task<IActionResult> CreateList()
        {
            var model = new NewListViewModel();
            return PartialView("_CreateList", model);

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
