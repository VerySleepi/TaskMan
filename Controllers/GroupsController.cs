using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public async Task<IActionResult> GroupDashboard()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var members = await _context.GroupUsers
                .Where(m => m.User_ID == currentUser.Id)
                .ToListAsync();

            var groupIds = members.Select(m => m.Group_ID).ToList();

            var groups = await _context.Group
                .Where(g => groupIds.Contains(g.Group_ID))
                .ToListAsync();

            var model = new GroupsViewModel
            {
                Groups = groups,
                Members = members
            };

            ViewData["userName"] = currentUser.UserName;
            ViewData["profilePic"] = Url.Content($"~/images/{currentUser.Profile_Pic}");
            ViewData["trashIcon"] = Url.Content("~/images/trash-can-solid.png");
            ViewData["editIcon"] = Url.Content("~/images/pencil-solid.png");

            return View("GroupDashboard", model);
        }

        // GET: GroupsController/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int groupId)
        {
            // Check if user exists
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("GroupDashboard");
            }

            // Check if user is a member of the group
            var membership = await _context.GroupUsers
                .FirstOrDefaultAsync(m => m.User_ID == currentUser.Id && m.Group_ID == groupId);
            if (membership == null)
            {
                return RedirectToAction("GroupDashboard");
            }

            // Get group details for the current group
            var group = await _context.Group
                .FirstOrDefaultAsync(g => g.Group_ID == groupId);

            // Get all lists of tasks for the group
            var groupLists = await _context.GroupList
                .Where(gl => gl.Group_ID == groupId)
                .ToListAsync();

            // Extract Group_List_IDs from the GroupLists
            var listIds = groupLists.Select(gl => gl.Group_List_ID).ToList();

            // Fetch tasks that belong to the lists of the current group
            var groupTasks = await _context.GroupTask
                .Where(gt => listIds.Contains(gt.Group_List_ID))
                .ToListAsync();

            // Organise tasks into dictionary with key of listId
            // so the view can do TasksByList[Group_List_ID] to get the tasks for that list
            var tasksByList = groupTasks
                .GroupBy(gt => gt.Group_List_ID)
                .ToDictionary(g => g.Key, g => g.ToList());

            var model = new GroupDetailViewModel
            {
                Group = group,
                GroupLists = groupLists,
                TasksByList = tasksByList,
                UserRole = membership.Role,
                User_ID = currentUser.Id
            };

            return View("GroupDetail", model);
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
                return RedirectToAction("GroupDashboard");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error creating group: " + ex.Message);
                return RedirectToAction("GroupDashboard");
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
