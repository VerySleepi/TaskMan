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
            ViewData["trashIcon"] = Url.Content("~/images/trash-can-solid.png");
            ViewData["editIcon"] = Url.Content("~/images/pencil-solid.png");


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
                    Tasks = tasksList.Where(t => t.User_List_ID == ut.User_List_ID)
                    .OrderBy(t => t.Status)
                    .ToList()
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
                .Where(ut => ut.User_ID == userId)
                .ToListAsync();


            var model = new NewTaskViewModel
            {
                Due_Date = DateOnly.FromDateTime(DateTime.Now),
                Status = 0,
                User_List_ID = userTaskLists[0].User_List_ID,
                UserTaskLists = userTaskLists
            };

            ViewData["Context"] = "Create";

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

                var task = new Models.Task
                {
                    Title = model.Title,
                    Description = model.Description,
                    Due_Date = model.Due_Date,
                    Status = 0,
                    User_List_ID = model.User_List_ID
                };

                Debug.WriteLine(task.Title);
                Debug.WriteLine(task.Description);
                Debug.WriteLine(task.Due_Date);
                Debug.WriteLine(task.Status);
                Debug.WriteLine(task.User_List_ID);

                _context.Task.Add(task);
                await _context.SaveChangesAsync();
                return RedirectToAction("Dashboard");
            }
            catch
            {
                return RedirectToAction("Dashboard");
            }
        }



        // GET: TasksController/EditTask/5
        [HttpGet]
        public async Task<IActionResult> EditTask(int taskId)
        {
            var userId = userManager.GetUserId(HttpContext.User);

            var task = await _context.Task.FindAsync(taskId);
            if (task == null)
            {
                Debug.WriteLine($"Task with ID {taskId} not found.");
                return NotFound();
            }

            // Fetch task parent UserTasks by task.User_List_ID
            var taskList = await _context.UserTasks.FindAsync(task.User_List_ID);
            if (taskList == null)
            {
                Debug.WriteLine($"UserTasks with ID {task.User_List_ID} not found.");
                return NotFound();
            }
            // Ownership check: Ensure the current user owns the task list that the task belongs to
            if (taskList.User_ID != userId)
            {
                Debug.WriteLine($"Unauthorized attempt to edit task with ID {taskId} by user {userId}.");
                return Forbid();
            }


            var userTaskLists = await _context.UserTasks
                .Where(ut => ut.User_ID == userId && ut.Title != "Completed Tasks")
                .ToListAsync();


            var model = new NewTaskViewModel
            {
                Task_ID = task.Task_ID,
                Title = task.Title,
                Description = task.Description,
                Due_Date = task.Due_Date,
                Status = 0,
                User_List_ID = task.User_List_ID,
                UserTaskLists = userTaskLists
            };

            Debug.WriteLine($"Task ID: {model.Task_ID}, Title: {model.Title}, Description: {model.Description}, Due Date: {model.Due_Date}, Status: {model.Status}, User List ID: {model.User_List_ID}");

            ViewData["Context"] = "Edit";
            return PartialView("_CreateTask", model);
        }

        // POST: TasksController/EditTask/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTask(int taskId, NewTaskViewModel model)
        {
            try
            {
                var userId = userManager.GetUserId(HttpContext.User);

                var task = await _context.Task.FindAsync(taskId);
                if (task == null)
                {
                    Debug.WriteLine($"Task with ID {taskId} not found.");
                    return NotFound();
                }

                // Fetch parent UserTasks by task.User_List_ID
                var taskList = await _context.UserTasks.FindAsync(task.User_List_ID);

                if (taskList == null)
                {
                    Debug.WriteLine($"UserTasks with ID {task.User_List_ID} not found.");
                    return NotFound();
                }
                // Ownership check: Ensure the current user owns the task list that the task belongs to
                if (taskList.User_ID != userId)
                {
                    Debug.WriteLine($"Unauthorized attempt to delete task with ID {taskId} by user {userId}.");
                    return Forbid();
                }

                task.Title = model.Title;
                task.Description = model.Description;
                task.Due_Date = model.Due_Date;
                task.Status = model.Status;
                var changedTaskList = await _context.UserTasks.FindAsync(model.User_List_ID);
                if (changedTaskList == null)
                {
                    Debug.WriteLine($"UserTasks with ID {model.User_List_ID} not found.");
                    return NotFound();
                }
                if (changedTaskList.User_ID != userId)
                {
                    Debug.WriteLine($"Unauthorized attempt to change task list to ID {model.User_List_ID} for task with ID {taskId} by user {userId}.");
                    return Forbid();
                }
                else
                {
                    Debug.WriteLine($"User {userId} is authorized to change task list to ID {model.User_List_ID} for task with ID {taskId}.");
                    task.User_List_ID = model.User_List_ID;
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("Dashboard", "Tasks");
            }
            catch
            {
                Debug.WriteLine($"An error occurred while trying to edit task with ID {taskId}.");
                return RedirectToAction("Dashboard", "Tasks");
            }
        }

        [HttpPost]
        public async Task<IActionResult> TaskCompletion(int taskId)
        {
            try
            {
                var userId = userManager.GetUserId(HttpContext.User);

                var task = await _context.Task.FindAsync(taskId);
                if (task == null)
                {
                    Debug.WriteLine($"Task with ID {taskId} not found.");
                    return NotFound();
                }
                // Fetch task parent UserTasks by task.User_List_ID
                var taskList = await _context.UserTasks.FindAsync(task.User_List_ID);
                if (taskList == null)
                {
                    Debug.WriteLine($"UserTasks with ID {task.User_List_ID} not found.");
                    return NotFound();
                }
                // Ownership check: Ensure the current user owns the task list that the task belongs to
                if (taskList.User_ID != userId)
                {
                    Debug.WriteLine($"Unauthorized attempt to change completion status of task with ID {taskId} by user {userId}.");
                    return Forbid();
                }
                // Toggle task completion status => Update list progress if task is being marked as completed
                task.Status = task.Status == 0 ? 1 : 0;

                // Count total number of tasks for this task list
                var totalTasks = await _context.Task.Where(t => t.User_List_ID == task.User_List_ID).CountAsync();
                // Count number of completed tasks for this task list
                var completedTasks = await _context.Task.Where(t => t.User_List_ID == task.User_List_ID && t.Status == 1).CountAsync();
                // Update task list progress based on the number of completed tasks
                taskList.Progress = totalTasks > 0 ? Convert.ToInt32(Math.Round(100.0 * (completedTasks / totalTasks))) : 100;

                await _context.SaveChangesAsync();
                return RedirectToAction("Dashboard", "Tasks");

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred while trying to change completion status of task with ID {taskId}.", ex);
                return RedirectToAction("Dashboard", "Tasks");
            }
        }

        // POST: TasksController/DeleteTask/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTask(int taskId)
        {
            try
            {
                var userId = userManager.GetUserId(HttpContext.User);

                // Fetch task by id
                var task = await _context.Task.FindAsync(taskId);
                if (task == null)
                {
                    Debug.WriteLine($"Task with ID {taskId} not found.");
                    return NotFound();
                }

                // Fetch parent UserTasks by task.User_List_ID
                var taskList = await _context.UserTasks.FindAsync(task.User_List_ID);

                if (taskList == null)
                {
                    Debug.WriteLine($"UserTasks with ID {task.User_List_ID} not found.");
                    return NotFound();
                }
                // Ownership check: Ensure the current user owns the task list that the task belongs to
                if (taskList.User_ID != userId)
                {
                    Debug.WriteLine($"Unauthorized attempt to delete task with ID {taskId} by user {userId}.");
                    return Forbid();
                }

                Debug.WriteLine($"User {userId} is authorized to delete task with ID {taskId}.");
                _context.Task.Remove(task);
                await _context.SaveChangesAsync();

                return RedirectToAction("Dashboard", "Tasks");
            }
            catch (Exception ex)
            {
                {
                    Debug.WriteLine($"An error occurred while trying to delete task with ID {taskId}.", ex);
                    return RedirectToAction("Dashboard", "Tasks");
                }
            }
        }

        // Task List CRUD Operations

        // GET: TasksController/CreateList
        [HttpGet]
        public async Task<IActionResult> CreateList()
        {
            var userID = userManager.GetUserId(HttpContext.User);
            if (userID == null)
            {
                return RedirectToAction("Dashboard");
            }

            var model = new NewListViewModel
            {
                Progress = 100,
                BckGr_Col = "#E67E75"
            };
            return PartialView("_CreateList", model);
        }

        // POST: TasksController/CreateList
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateList(NewListViewModel listViewModel)
        {
            var userID = userManager.GetUserId(HttpContext.User);
            var userTaskLists = await _context.UserTasks.Where(t => t.User_ID == userID && t.Title.StartsWith("Task List")).ToListAsync();
            try
            {
                //if (!ModelState.IsValid)
                //{
                //    return PartialView("_CreateList", listViewModel);
                //}

                if (listViewModel.Title == null)
                {
                    if (userTaskLists.Count > 0)
                    {
                        listViewModel.Title = $"Task List {userTaskLists.Count}";
                    }
                    else
                    {
                        listViewModel.Title = "Task List";
                    }
                }
                if (listViewModel.Description == null)
                {
                    listViewModel.Description = "No Description";
                }


                var taskList = new Models.UserTasks
                {
                    User_ID = userID,
                    Title = listViewModel.Title,
                    Description = listViewModel.Description,
                    Progress = listViewModel.Progress,
                    BckGr_Col = listViewModel.BckGr_Col
                };
                _context.UserTasks.Add(taskList);
                await _context.SaveChangesAsync();
                return RedirectToAction("Dashboard", "Tasks");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return RedirectToAction("Dashboard", "Tasks");
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteList(int listId)
        {
            try
            {
                var userId = userManager.GetUserId(HttpContext.User);

                // Fetch task list by id
                var taskList = await _context.UserTasks.FindAsync(listId);
                if (taskList == null)
                {
                    Debug.WriteLine($"UserTasks with ID {listId} not found.");
                    return NotFound();
                }

                // Ownership check: Ensure the current user owns the task list
                if (taskList.User_ID != userId)
                {
                    Debug.WriteLine($"Unauthorized attempt to delete task list with ID {listId} by user {userId}.");
                    return Forbid();
                }

                var tasks = await _context.Task.Where(t => t.User_List_ID == listId).ToListAsync();

                _context.Task.RemoveRange(tasks);
                _context.UserTasks.Remove(taskList);
                await _context.SaveChangesAsync();

                return RedirectToAction("Dashboard", "Tasks");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return RedirectToAction("Dashboard", "Tasks");
            }
        }
    }
}
