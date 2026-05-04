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

        // GET: GroupsDashboard
        public async Task<IActionResult> GroupDashboard()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            // Groups the user is a member of from GroupUsers table
            var memberOf = await _context.GroupUsers
                .Where(m => m.User_ID == currentUser.Id)
                .ToListAsync();

            // Ids of the groups the user is a member of
            var groupIds = memberOf.Select(m => m.Group_ID).ToList();

            // Groups the user is a member of from Group table
            var groups = await _context.Group
                .Where(g => groupIds.Contains(g.Group_ID))
                .ToListAsync();

            var model = new GroupsViewModel
            {
                Groups = groups,
                Members = memberOf
            };

            ViewData["userName"] = currentUser.UserName;
            ViewData["profilePic"] = Url.Content($"~/images/{currentUser.Profile_Pic}");
            ViewData["trashIcon"] = Url.Content("~/images/trash-can-solid.png");
            ViewData["editIcon"] = Url.Content("~/images/pencil-solid.png");

            return View("GroupDashboard", model);
        }

        // Helper method to build GroupDetailViewModel for the Details, AddMembers and RemoveMembers actions to avoid code duplication
        private async Task<GroupDetailViewModel> BuildGroupDetailViewModel(int groupId)
        {
            // Current user record
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return null;
            }

            // All records of the members of the current group from GroupUsers table.
            var groupMembers = await _context.GroupUsers
                .Where(m => m.Group_ID == groupId)
                .ToListAsync();

            // Ids of the members of the current group.
            var groupMemberIds = groupMembers.Select(m => m.User_ID).ToList();
            // All user records of the members of the current group.
            var groupUsers = await _userManager.Users
                .Where(u => groupMemberIds.Contains(u.Id))
                .ToListAsync();

            // Combined user records and their corresponding role in the current group.
            var userMembers = new List<MemberDetails>();

            foreach (var member in groupMembers)
            {
                var user = groupUsers.FirstOrDefault(u => u.Id == member.User_ID);
                if (user != null)
                {
                    userMembers.Add(new MemberDetails
                    {
                        User = user,
                        Role = member.Role,
                    });
                    continue;
                }
            }

            var nonMembers = await _userManager.Users
                .Where(u => !groupMemberIds.Contains(u.Id))
                .ToListAsync();


            //  The record of the current user's membership of the current group from GroupUsers table. Used to check if user is member of the group and to get the user's role in the group
            var memberOf = groupMembers
                .FirstOrDefault(m => m.User_ID == currentUser.Id);

            // Check if user member of any group
            if (memberOf == null)
            {
                return null;
            }

            // All details of the current group
            var group = await _context.Group
                .FirstOrDefaultAsync(g => g.Group_ID == groupId);

            // All task lists that belong to the current group
            var groupLists = await _context.GroupList
                .Where(gl => gl.Group_ID == groupId)
                .ToListAsync();

            // Extracted Group_List_IDs from the GroupLists of the current group. Used to fetch tasks that belong to the lists of the current group
            var listIds = groupLists.Select(gl => gl.Group_List_ID).ToList();

            // Fetch tasks that belong to the lists of the current group
            var groupTasks = await _context.GroupTask
                .Where(gt => listIds.Contains(gt.Group_List_ID))
                .ToListAsync();

            // Organise tasks by their Group_List_ID for easy access in the view
            var tasksByList = groupTasks
                .GroupBy(gt => gt.Group_List_ID)
                .ToDictionary(g => g.Key, g => g.ToList());

            var model = new GroupDetailViewModel
            {
                Group = group,
                GroupLists = groupLists,
                TasksByList = tasksByList,
                UserRole = memberOf.Role,
                User_ID = currentUser.Id,
                Members = userMembers,
                NonMembers = nonMembers
            };

            return model;
        }

        // GET: GroupsController/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int groupId)
        {
            var model = await BuildGroupDetailViewModel(groupId);

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

                if (model.Title == null)
                {
                    model.Title = "New Group";
                }

                if (model.Description == null)
                {
                    model.Description = "";
                }

                var group = new Group
                {
                    Title = model.Title,
                    Description = model.Description
                };

                _context.Group.Add(group);
                _context.SaveChanges();



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
                Debug.WriteLine("Error creating group: ");
                return RedirectToAction("GroupDashboard");
            }
        }

        // GET: GroupsController/AddMembers/5
        [HttpGet]
        public async Task<IActionResult> AddMembers(int groupId)
        {
            var model = await BuildGroupDetailViewModel(groupId);
            if (model == null)
            {
                return RedirectToAction("GroupDashboard");
            }
            ViewData["Context"] = "AddMembers";
            return PartialView("Popups/_AddMembers", model);
        }

        // POST: GroupsController/AddMembers/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMembers(int groupId, List<string> userIds)
        {
            try
            {
                ViewData["Context"] = "AddMembers";
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    Debug.WriteLine("Current user not found");
                    return RedirectToAction("Details", new { groupId });
                }

                var userMembership = await _context.GroupUsers
                    .FirstOrDefaultAsync(m => m.User_ID == currentUser.Id && m.Group_ID == groupId);

                if (userMembership == null || userMembership.Role != "Admin")
                {
                    Debug.WriteLine("Current user is not an admin of the group");
                    return RedirectToAction("Details", new { groupId });
                }


                foreach (var userId in userIds)
                {
                    var existingMembers = await _context.GroupUsers
                        .AnyAsync(m => m.User_ID == userId && m.Group_ID == groupId);

                    if (existingMembers)
                    {
                        Debug.WriteLine("User is already a member of the group");
                        continue;
                    }

                    var newMember = new GroupUsers
                    {
                        User_ID = userId,
                        Group_ID = groupId,
                        Role = "Member",
                        Join_Date = DateOnly.FromDateTime(DateTime.Today)
                    };

                    _context.GroupUsers.Add(newMember);

                }
                await _context.SaveChangesAsync();

                return RedirectToAction("Details", new { groupId });

            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error adding member: " + ex.Message);
                return RedirectToAction("Details", new { groupId });
            }
        }

        // GET: GroupsController/RemoveMembers/5
        [HttpGet]
        public async Task<IActionResult> RemoveMembers(int groupId)
        {
            var model = await BuildGroupDetailViewModel(groupId);
            if (model == null)
            {
                return RedirectToAction("GroupDashboard");
            }
            ViewData["Context"] = "RemoveMembers";
            return PartialView("Popups/_RemoveMembers", model);
        }

        // POST: GroupsController/RemoveMembers/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMembers(int groupId, List<string> userIds)
        {
            try
            {
                ViewData["Context"] = "RemoveMembers";
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    Debug.WriteLine("Current user not found");
                    return RedirectToAction("Details", new { groupId });
                }

                var userMembership = await _context.GroupUsers
                    .FirstOrDefaultAsync(m => m.User_ID == currentUser.Id && m.Group_ID == groupId);

                if (userMembership == null || userMembership.Role != "Admin")
                {
                    Debug.WriteLine("Current user is not an admin of the group");
                    return RedirectToAction("Details", new { groupId });
                }

                foreach (var userId in userIds)
                {
                    var memberToRemove = await _context.GroupUsers
                    .FirstOrDefaultAsync(m => m.User_ID == userId && m.Group_ID == groupId);
                    if (memberToRemove == null || memberToRemove.Role == "Admin")
                    {
                        Debug.WriteLine($"User {userId} is not a member of the group or is an admin");
                        continue;
                    }

                    _context.GroupUsers.Remove(memberToRemove);
                }

                await _context.SaveChangesAsync();

                return RedirectToAction("Details", new { groupId });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error removing member: " + ex.Message);
                return RedirectToAction("Details", new { groupId });
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
