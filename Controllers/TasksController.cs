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
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
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
                // For each UserTasks, find the corresponding Tasks and create UserTasksGrouped objects, then order the tasks by status (incomplete first)
                // and add it to the structuredUserTasks list
                StructuredUserTasks = userTasksList.Select(ut => new UserTasksGrouped
                {
                    UserTask = ut,
                    Tasks = tasksList.Where(t => t.User_List_ID == ut.User_List_ID)
                    .OrderBy(t => t.Status)
                    .ToList()
                }).ToList(),
            };
            return View(model);
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
            var userID = userManager.GetUserId(HttpContext.User);

            // Fetch Tasks that match the extracted User_List_IDs
            var tasksList = await _context.Task
                .Where(t => t.User_List_ID == model.User_List_ID && t.Title == model.Title)
                .ToListAsync();
            try
            {
                // Set default title if none provided
                if (model.Title == null || model.Title == string.Empty)
                {
                    model.Title = "New Task";
                }

                var existingTaskCount = await _context.UserTasks.CountAsync(ut => ut.User_ID == userID && ut.Title == model.Title);

                if (existingTaskCount > 0)
                {
                    // Add number of tasks with same title as what was entered after task title
                    model.Title = $"{model.Title} ({existingTaskCount})";

                    if (_context.UserTasks.Any(ut => ut.User_ID == userID && ut.Title == model.Title))
                    {
                        model.Title = $"{model.Title} ({existingTaskCount + 1})";
                    }
                }



                // Set default description if none provided
                if (model.Description == null)
                {
                    model.Description = "No Description";
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
                await UpdateTaskListProgress(model.User_List_ID);

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
            }

            var userTaskLists = await _context.UserTasks
                .Where(ut => ut.User_ID == userId)
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
                }

                if (model.Title == null || model.Title == string.Empty)
                {
                    model.Title = "New Task";
                }

                // Fetch parent UserTasks by task.User_List_ID
                var taskList = await _context.UserTasks.FindAsync(task.User_List_ID);

                if (taskList == null)
                {
                    Debug.WriteLine($"UserTasks with ID {task.User_List_ID} not found.");
                }
                // Ownership check: Ensure the current user owns the task list that the task belongs to
                if (taskList.User_ID != userId)
                {
                    Debug.WriteLine($"Unauthorized attempt to delete task with ID {taskId} by user {userId}.");
                    return Forbid();
                }

                var existingTaskCount = await _context.UserTasks.CountAsync(ut => ut.User_ID == userId && ut.Title == model.Title);

                if (existingTaskCount > 0)
                {
                    // Add number of tasks with same title as what was entered after task title
                    model.Title = $"{model.Title} ({existingTaskCount})";

                    if (_context.UserTasks.Any(ut => ut.User_ID == userId && ut.Title == model.Title))
                    {
                        model.Title = $"{model.Title} ({existingTaskCount + 1})";
                    }
                }

                task.Title = model.Title;
                task.Description = model.Description;
                task.Due_Date = model.Due_Date;
                task.Status = model.Status;

                var changedTaskList = await _context.UserTasks.FindAsync(model.User_List_ID);
                if (changedTaskList == null)
                {
                    // Case for if the list doesn't exist, but should be caught by catch block, but added here just in case
                    Debug.WriteLine($"UserTasks with ID {model.User_List_ID} not found.");
                }
                if (changedTaskList.User_ID != userId)
                {
                    // If user somehow tries to change the task list to one that they don't own, return a 403 Forbidden response
                    Debug.WriteLine($"Unauthorized attempt to change task list to ID {model.User_List_ID} for task with ID {taskId} by user {userId}.");
                    return Forbid();
                }
                else
                {
                    Debug.WriteLine($"User {userId} is authorized to change task list to ID {model.User_List_ID} for task with ID {taskId}.");
                    task.User_List_ID = model.User_List_ID;
                }

                await _context.SaveChangesAsync();
                await UpdateTaskListProgress(taskList.User_List_ID);
                await UpdateTaskListProgress(model.User_List_ID);

                return RedirectToAction("Dashboard", "Tasks");
            }
            catch
            {
                Debug.WriteLine($"An error occurred while trying to edit task with ID {taskId}.");
                return RedirectToAction("Dashboard", "Tasks");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TaskCompletion(int taskId)
        {
            try
            {
                var user = await userManager.GetUserAsync(HttpContext.User);
                if (user == null)
                {
                    Debug.WriteLine("User not found.");
                    return RedirectToAction("Login", "Account");
                }

                var task = await _context.Task.FindAsync(taskId);
                if (task == null)
                {
                    Debug.WriteLine($"Task with ID {taskId} not found.");
                }
                // Fetch task parent UserTasks by task.User_List_ID
                var taskList = await _context.UserTasks.FindAsync(task.User_List_ID);
                if (taskList == null)
                {
                    Debug.WriteLine($"UserTasks with ID {task.User_List_ID} not found.");
                }
                // Ownership check: Ensure the current user owns the task list that the task belongs to
                if (taskList.User_ID != user.Id)
                {
                    Debug.WriteLine($"Unauthorized attempt to change completion status of task with ID {taskId} by user {user.Id}.");
                    return Forbid();
                }
                // Toggle task completion status => Update list progress if task is being marked as completed or incompleted
                task.Status = task.Status == 0 ? 1 : 0;
                await _context.SaveChangesAsync();

                if (task.Status == 1)
                {
                    user.Complete_Tasks += 1;
                }
                else
                {
                    user.Complete_Tasks -= 1;
                }

                await UpdateTaskListProgress(task.User_List_ID);

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
                }

                // Fetch parent UserTasks by task.User_List_ID
                var taskList = await _context.UserTasks.FindAsync(task.User_List_ID);

                if (taskList == null)
                {
                    Debug.WriteLine($"UserTasks with ID {task.User_List_ID} not found.");
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

                await UpdateTaskListProgress(task.User_List_ID);

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
                Progress = 0,
                BckGr_Col = "#75E67E",
            };
            return PartialView("_CreateList", model);
        }

        // POST: TasksController/CreateList
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateList(NewListViewModel listViewModel)
        {
            // Check if user is authenticated and get user ID
            var userID = userManager.GetUserId(HttpContext.User);
            if (userID == null)
            {
                return RedirectToAction("Dashboard", "Tasks");
            }
            try
            {
                // Set default title if none provided
                if (listViewModel.Title == null || listViewModel.Title == string.Empty)
                {
                    listViewModel.Title = "New List";
                }

                var existingListCount = await _context.UserTasks.CountAsync(ut => ut.User_ID == userID && ut.Title == listViewModel.Title);

                if (existingListCount > 0)
                {
                    // Add number of lists with same title as what was entered after list title
                    listViewModel.Title = $"{listViewModel.Title} ({existingListCount})";

                    // If there is already a list with the same title and number, add 1 to the number and update the title again
                    if (_context.UserTasks.Any(ut => ut.User_ID == userID && ut.Title == listViewModel.Title))
                    {
                        listViewModel.Title = $"{listViewModel.Title} ({existingListCount + 1})";
                    }
                }

                // Set default description if none provided
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
                    return RedirectToAction("Dashboard");
                }

                // Ownership check: Ensure the current user owns the task list so someone
                // can't delete other users lists by manipulating the URL
                if (taskList.User_ID != userId)
                {
                    Debug.WriteLine($"Unauthorized attempt to delete task list with ID {listId} by user {userId}.");
                    return Forbid();
                }

                var tasks = await _context.Task.Where(t => t.User_List_ID == listId).ToListAsync();

                _context.Task.RemoveRange(tasks);
                _context.UserTasks.Remove(taskList);
                await _context.SaveChangesAsync();

                return RedirectToAction("Dashboard");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return RedirectToAction("Dashboard");
            }
        }

        // Helper method to update task list progress based on the completion status of its tasks
        public async System.Threading.Tasks.Task UpdateTaskListProgress(int listId)
        {
            var taskList = await _context.UserTasks.FindAsync(listId);
            if (taskList == null)
            {
                Debug.WriteLine($"UserTasks with ID {listId} not found.");
                return;
            }

            var tasks = await _context.Task.Where(t => t.User_List_ID == listId).ToListAsync();

            if (tasks.Count == 0)
            {
                taskList.Progress = 0;
            }
            else
            {
                // Calculate number of completed tasks and update the progress percentage of the task list
                var completedTasks = tasks.Where(t => t.Status == 1).Count();
                taskList.Progress = Convert.ToInt32(Math.Round(100.0 * (completedTasks / Convert.ToDouble(tasks.Count))));
            }

            _context.UserTasks.Update(taskList);
            await _context.SaveChangesAsync();
        }

        // Moved Error action here due to removing HomeController
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
