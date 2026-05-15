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

            // Combine the groups and users membership details into a list of GroupCards to be used in the view model
            var groupCards = groups.Select(group => new GroupCard
            {
                Group = group,
                Member = memberOf.FirstOrDefault(m => m.Group_ID == group.Group_ID)
            }).ToList();

            var model = new GroupsViewModel
            {
                GroupCards = groupCards
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

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = await BuildGroupDetailViewModel(groupId);

            return View("GroupDetail", model);
        }

        // GET: GroupsController/Create
        [HttpGet]
        public IActionResult CreateGroup()
        {
            var model = new NewGroupViewModel();
            return PartialView("_CreateGroup", model);
        }

        // POST: GroupsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGroup(NewGroupViewModel model)
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
                    model.Description = "No Description";
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

                return RedirectToAction("GroupDashboard");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error creating group: ");
                return RedirectToAction("GroupDashboard");
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditGroup(int groupId)
        {
            var group = await _context.Group.FindAsync(groupId);
            if (group == null)
            {
                Debug.WriteLine($"Group with ID {groupId} not found");
                return RedirectToAction("GroupDashboard");
            }

            var model = new NewGroupViewModel
            {
                GroupId = group.Group_ID,
                Title = group.Title,
                Description = group.Description
            };
            return PartialView("_EditGroup", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditGroup(NewGroupViewModel model)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Account");
                }
                var group = await _context.Group.FindAsync(model.GroupId);
                if (group == null)
                {
                    Debug.WriteLine($"Group with ID {model.GroupId} not found");
                    return RedirectToAction("GroupDashboard");
                }

                var memberOf = await IsGroupAdmin(model.GroupId);

                if (!memberOf)
                {
                    return RedirectToAction("GroupDashboard");
                }

                if (model.Title == null || model.Title == string.Empty)
                {
                    model.Title = "New Group";
                }
                
                if (model.Description == null)
                {
                    model.Description = "No Description";
                }

                group.Title = model.Title;
                group.Description = model.Description;

                _context.Group.Update(group);
                await _context.SaveChangesAsync();
                return RedirectToAction("GroupDashboard");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while editing the group. Please try again.");
                return View(model);
            }
        }

        // POST: GroupsController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGroup(int groupId)
        {
            if (!await IsGroupAdmin(groupId))
            {
                return RedirectToAction("GroupDashboard");
            }

            try
            {
                var group = await _context.Group.FindAsync(groupId);
                if (group == null)
                {
                    Debug.WriteLine($"Group with ID {groupId} not found");
                    return RedirectToAction("GroupDashboard");
                }

                _context.Group.Remove(group);
                await _context.SaveChangesAsync();

                return RedirectToAction("GroupDashboard");

            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error deleting group: " + ex.Message);
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

        // POST: GroupsController/RemoveMembers/5/userIds
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMembers(int groupId, List<string> userIds)
        {
            try
            {
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

        // GET: GroupsController/CreateGroupTask/5
        [HttpGet]
        public async Task<IActionResult> CreateGroupTask(int groupId)
        {

            // List that is being added to by using its Group_ID
            var groupLists = await _context.GroupList
                .Where(gl => gl.Group_ID == groupId)
                .ToListAsync();

            if (groupLists.Count == 0)
            {
                return RedirectToAction("Details", new { groupId });
            }

            var groupList = groupLists.FirstOrDefault()?.Group_List_ID ?? 0;


            var memberOf = await IsGroupAdmin(groupId);

            if (!memberOf)
            {
                return RedirectToAction("Details", new { groupId });
            }

            var model = new NewGroupTaskViewModel
            {
                Group_List_ID = groupList,
                Group_ID = groupId,
                Due_Date = DateOnly.FromDateTime(DateTime.Now),
                Status = 0,
                GroupLists = groupLists
            };

            ViewData["Context"] = "Create";

            return PartialView("_CreateGroupTask", model);
        }

        // POST: GroupsController/CreateGroupTask/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGroupTask(NewGroupTaskViewModel model)
        {
            Debug.WriteLine($"Group_ID: {model.Group_ID}, Group_List_ID: {model.Group_List_ID}, Title: {model.Title}");
            // Fetch Tasks that match the extracted User_List_IDs
            var tasksList = await _context.GroupTask
                .Where(t => t.Group_List_ID == model.Group_List_ID && t.Title.StartsWith("New Task"))
                .ToListAsync();
            try
            {
                if (model.Title == null || model.Title == string.Empty)
                {

                    model.Title = "Task List";
                }

                var existingListCount = await _context.GroupTask.CountAsync(ut => ut.Group_List_ID == model.Group_List_ID && ut.Title == model.Title);

                if (existingListCount > 0)
                {
                    // Add number of tasks with same title as what was entered after task title
                    model.Title = $"{model.Title} ({existingListCount})";

                    // If there is already a task with the same title and number, add 1 to the number and update the title again
                    if (_context.GroupTask.Any(gl => gl.Group_List_ID == model.Group_List_ID && gl.Title == model.Title))
                    {
                        model.Title = $"{model.Title} ({existingListCount + 1})";
                    }
                }
                if (model.Description == null)
                {
                    model.Description = "No Description";
                }

                var groupList = await _context.GroupList
                    .FirstOrDefaultAsync(gl => gl.Group_List_ID == model.Group_List_ID);

                if (groupList == null)
                {
                    Debug.WriteLine($"Group list with ID {model.Group_List_ID} not found");
                    return RedirectToAction("Details", new { groupId = model.Group_ID });
                }

                var memberOf = await IsGroupAdmin(groupList.Group_ID);

                if (!memberOf)
                {
                    Debug.WriteLine("Current user is not an admin of the group");
                    return RedirectToAction("Details", new { groupId = model.Group_ID });
                }

                var newTask = new GroupTask
                {
                    Group_List_ID = model.Group_List_ID,
                    Title = model.Title,
                    Description = model.Description,
                    Due_Date = model.Due_Date,
                    Status = 0
                };

                _context.GroupTask.Add(newTask);
                await _context.SaveChangesAsync();

                UpdateGroupListProgress(model.Group_List_ID).Wait();

                return RedirectToAction("Details", new { groupId = groupList.Group_ID });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return RedirectToAction("GroupDashboard");
            }
        }

        // POST: GroupsController/MarkGroupTaskComplete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkGroupTaskComplete(int taskId)
        {
            try
            {
                var userId = _userManager.GetUserId(HttpContext.User);

                // Fetch task by taskId
                var task = await _context.GroupTask.FindAsync(taskId);
                if (task == null)
                {
                    Debug.WriteLine($"Task with ID {taskId} not found.");
                    return RedirectToAction("Details", new { groupId = task.GroupList.Group_ID });
                }

                // Fetch task parent GroupList by task.Group_List_ID
                var taskList = await _context.GroupList.FindAsync(task.Group_List_ID);
                if (taskList == null)
                {
                    Debug.WriteLine($"GroupList with ID {task.Group_List_ID} not found.");
                    return RedirectToAction("Details", new { groupId = taskList.Group_ID });
                }

                // Member check - ensure user is a member of the group that the task belongs to
                var memberOf = await _context.GroupUsers
                    .FirstOrDefaultAsync(m => m.User_ID == userId && m.Group_ID == taskList.Group_ID);

                if (memberOf == null)
                {
                    Debug.WriteLine($"Unauthorized attempt to change completion status of task with ID {taskId} by user {userId}.");
                    return RedirectToAction("GroupDashboard");
                }

                // Toggle task completion status => Update list progress if task is being marked as completed
                task.Status = task.Status == 0 ? 1 : 0;
                await _context.SaveChangesAsync();

                UpdateGroupListProgress(task.Group_List_ID).Wait();
                return RedirectToAction("Details", new { groupId = taskList.Group_ID });

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return RedirectToAction("GroupDashboard");
            }
        }

        // GET: GroupsController/DeleteGroupTask/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGroupTask(int taskId)
        {
            Debug.WriteLine($"DeleteGroupTask called with taskId: {taskId}");
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);

                if (currentUser == null)
                {
                    Debug.WriteLine("Current user not found");
                    return RedirectToAction("Login", "Account");
                }

                var task = await _context.GroupTask
                    .FirstOrDefaultAsync(t => t.Group_Task_ID == taskId);

                if (task == null)
                {
                    Debug.WriteLine($"Task with ID {taskId} not found");
                    return RedirectToAction("Details", new { groupId = task.GroupList.Group_ID });
                }

                var groupList = await _context.GroupList
                    .FirstOrDefaultAsync(gl => gl.Group_List_ID == task.Group_List_ID);

                if (groupList == null)
                {
                    Debug.WriteLine($"Group list with ID {task.Group_List_ID} not found");
                    return RedirectToAction("Details", new { groupId = task.GroupList.Group_ID });
                }

                var memberOf = await IsGroupAdmin(groupList.Group_ID);
                if (!memberOf)
                {
                    Debug.WriteLine("Current user is not an admin of the group");
                    return RedirectToAction("Details", new { groupId = groupList.Group_ID });
                }

                _context.GroupTask.Remove(task);
                await _context.SaveChangesAsync();

                UpdateGroupListProgress(task.Group_List_ID).Wait();

                return RedirectToAction("Details", new { groupId = groupList.Group_ID });

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return RedirectToAction("GroupDashboard");
            }
        }

        // GET: GroupsController/EditGroupTask/5
        [HttpGet]
        public async Task<IActionResult> EditGroupTask(int taskId)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var task = await _context.GroupTask
                .FirstOrDefaultAsync(t => t.Group_Task_ID == taskId);
            if (task == null)
            {
                Debug.WriteLine($"Task with ID {taskId} not found");
                return RedirectToAction("Details", new { groupId = task.GroupList.Group_ID });
            }

            var groupList = await _context.GroupList
                    .FirstOrDefaultAsync(gl => gl.Group_List_ID == task.Group_List_ID);

            if (groupList == null)
            {
                Debug.WriteLine($"Group list with ID {task.Group_List_ID} not found");
                return RedirectToAction("Details", new { groupId = task.GroupList.Group_ID });
            }

            var memberOf = await IsGroupAdmin(groupList.Group_ID);

            if (!memberOf)
            {
                Debug.WriteLine("Current user is not an admin of the group");
                return RedirectToAction("Details", new { groupId = groupList.Group_ID });
            }

            var groupLists = await _context.GroupList
                .Where(gl => gl.Group_ID == groupList.Group_ID)
                .ToListAsync();

            var model = new NewGroupTaskViewModel
            {
                Group_Task_ID = task.Group_Task_ID,
                Title = task.Title,
                Description = task.Description,
                Due_Date = task.Due_Date,
                Status = task.Status,
                Group_List_ID = task.Group_List_ID,
                Group_ID = groupList.Group_ID,
                GroupLists = groupLists
            };

            ViewData["Context"] = "Edit";
            return PartialView("_CreateGroupTask", model);
        }

        // POST: GroupsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditGroupTask(int taskId, NewGroupTaskViewModel model)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var task = await _context.GroupTask
                    .FirstOrDefaultAsync(t => t.Group_Task_ID == taskId);

                if (task == null)
                {
                    Debug.WriteLine($"Task with ID {taskId} not found");
                    return RedirectToAction("Details", new { groupId = task.GroupList.Group_ID });
                }

                var groupList = await _context.GroupList
                    .FirstOrDefaultAsync(gl => gl.Group_List_ID == task.Group_List_ID);

                if (groupList == null)
                {
                    Debug.WriteLine($"Group list with ID {task.Group_List_ID} not found");
                    return RedirectToAction("Details", new { groupId = task.GroupList.Group_ID });
                }

                var memberOf = await IsGroupAdmin(groupList.Group_ID);

                if (!memberOf)
                {
                    Debug.WriteLine("Current user is not an admin of the group");
                    return RedirectToAction("Details", new { groupId = groupList.Group_ID });
                }

                if (model.Title == null || model.Title == string.Empty)
                {

                    model.Title = "Task List";
                }

                var existingListCount = await _context.GroupTask.CountAsync(ut => ut.Group_List_ID == model.Group_List_ID && ut.Title == model.Title);

                if (existingListCount > 0)
                {
                    // Add number of tasks with same title as what was entered after task title
                    model.Title = $"{model.Title} ({existingListCount})";

                    // If there is already a task with the same title and number, add 1 to the number and update the title again
                    if (_context.GroupTask.Any(gl => gl.Group_List_ID == model.Group_List_ID && gl.Title == model.Title))
                    {
                        model.Title = $"{model.Title} ({existingListCount + 1})";
                    }
                }
                if (model.Description == null)
                {
                    model.Description = "No Description";
                }

                task.Title = model.Title;
                task.Description = model.Description;
                task.Due_Date = model.Due_Date;

                task.Status = 0;
                UpdateGroupListProgress(groupList.Group_List_ID).Wait();
                var changedTaskList = await _context.GroupList.FirstOrDefaultAsync(gl => gl.Group_List_ID == model.Group_List_ID && gl.Group_ID == groupList.Group_ID);
                if (changedTaskList == null)
                {
                    Debug.WriteLine($"Group list with ID {model.Group_List_ID} not found.");
                    return RedirectToAction("Details", new { groupId = groupList.Group_ID });
                }
                else
                {
                    Debug.WriteLine($"User {currentUser.Id} is authorized to change task list to ID {model.Group_List_ID} for task with ID {taskId}.");
                    task.Group_List_ID = model.Group_List_ID;
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("Details", new { groupId = groupList.Group_ID });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return RedirectToAction("Details", new { groupId = model.Group_ID });
            }
        }

        // GroupList CRUD operations

        // GET: GroupsController/CreateGroupList/5
        [HttpGet]
        public async Task<IActionResult> CreateGroupList(int groupId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var memberOf = await IsGroupAdmin(groupId);

            if (!memberOf)
            {
                Debug.WriteLine("Current user is not an admin of the group");
                return RedirectToAction("Details", new { groupId });
            }

            var model = new NewGroupListViewModel
            {
                Group_ID = groupId,
                BckGr_Col = "#75E67E",
            };

            return PartialView("_CreateGroupList", model);
        }

        // POST: GroupsController/CreateGroupList/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGroupList(NewGroupListViewModel model)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);

                var memberOf = await IsGroupAdmin(model.Group_ID);

                var groupLists = await _context.GroupList
                    .Where(gl => gl.Group_ID == model.Group_ID && gl.Title.StartsWith("Task List"))
                    .ToListAsync();

                if (!memberOf)
                {
                    Debug.WriteLine("Current user is not an admin of the group");
                    return RedirectToAction("Details", new { groupId = model.Group_ID });
                }

                if (model.Title == null || model.Title == string.Empty)
                {

                    model.Title = "Task List";
                }

                var existingListCount = await _context.GroupList.CountAsync(ut => ut.Group_ID == model.Group_ID && ut.Title == model.Title);

                if (existingListCount > 0)
                {
                    // Add number of lists with same title as what was entered after list title
                    model.Title = $"{model.Title} ({existingListCount})";

                    // If there is already a list with the same title and number, add 1 to the number and update the title again
                    if (_context.GroupList.Any(gl => gl.Group_ID == model.Group_ID && gl.Title == model.Title))
                    {
                        model.Title = $"{model.Title} ({existingListCount + 1})";
                    }
                }

                if (model.Description == null)
                {
                    model.Description = "No Description";
                }

                var newList = new GroupList
                {
                    Group_ID = model.Group_ID,
                    Title = model.Title,
                    Description = model.Description,
                    BckGr_Col = model.BckGr_Col,
                    Progress = 0
                };

                _context.GroupList.Add(newList);
                await _context.SaveChangesAsync();

                return RedirectToAction("Details", new { groupId = model.Group_ID });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error creating group list: " + ex.Message);
                return RedirectToAction("Details", new { groupId = model.Group_ID });
            }
        }

        // GET: GroupsController/DeleteGroupList/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGroupList(int groupListId)
        {
            var groupList = await _context.GroupList
                .FirstOrDefaultAsync(gl => gl.Group_List_ID == groupListId);

            if (groupList == null)
            {
                Debug.WriteLine($"Group list with ID {groupListId} not found");
                return RedirectToAction("Details", new { groupId = groupList.Group_ID });
            }
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);

                var memberOf = await IsGroupAdmin(groupList.Group_ID);

                if (!memberOf)
                {
                    Debug.WriteLine("Current user is not an admin of the group");
                    return RedirectToAction("Details", new { groupId = groupList.Group_ID });
                }

                // All tasks that belong to the group list being deleted.
                var tasks = await _context.GroupTask
                    .Where(gt => gt.Group_List_ID == groupListId)
                    .ToListAsync();

                _context.GroupTask.RemoveRange(tasks);
                _context.GroupList.Remove(groupList);

                await _context.SaveChangesAsync();

                return RedirectToAction("Details", new { groupId = groupList.Group_ID });

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return RedirectToAction("Details", new { groupId = groupList.Group_ID });
            }
        }

        // Helper Functions
        private async Task<bool> IsGroupAdmin(int groupId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return false;
            }
            var memberOf = await _context.GroupUsers
                .FirstOrDefaultAsync(m => m.User_ID == currentUser.Id && m.Group_ID == groupId);
            return memberOf != null && memberOf.Role == "Admin";
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
                        GroupUser = member
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


            var model = new GroupDetailViewModel
            {
                Group = group,
                GroupLists = groupLists,
                TasksByList = groupTasks.Select(t => new TasksByList
                {
                    GroupList = t.GroupList,
                    Tasks = groupTasks.Where(gt => gt.Group_List_ID == t.GroupList.Group_List_ID)
                                       .OrderBy(gt => gt.Status)
                                       .ToList()

                }).ToList(),
                UserRole = memberOf.Role,
                User_ID = currentUser.Id,
                Members = userMembers,
                NonMembers = nonMembers
            };

            ViewData["profilePic"] = Url.Content($"~/images/{currentUser.Profile_Pic}");
            ViewData["userName"] = currentUser.UserName;
            ViewData["trashIcon"] = Url.Content("~/images/trash-can-solid.png");
            ViewData["editIcon"] = Url.Content("~/images/pencil-solid.png");

            return model;
        }

        private async System.Threading.Tasks.Task UpdateGroupListProgress(int groupListId)
        {
            var groupList = await _context.GroupList.FindAsync(groupListId);

            if (groupList == null)
            {
                Debug.WriteLine($"Group list with ID {groupListId} not found");
                return;
            }

            var tasks = await _context.GroupTask
                .Where(t => t.Group_List_ID == groupListId)
                .ToListAsync();

            if (tasks.Count == 0)
            {
                groupList.Progress = 0;
            }
            else
            {
                var completedTasks = tasks.Where(t => t.Status == 1).Count();
                groupList.Progress = Convert.ToInt32(Math.Round(100.0 * (completedTasks / Convert.ToDouble(tasks.Count))));
            }

            _context.GroupList.Update(groupList);
            await _context.SaveChangesAsync();

        }
    }
}
