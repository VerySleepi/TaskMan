//using Microsoft.AspNetCore.Mvc;
//using TaskMan.Models;
//using TaskMan.Data;
//using Microsoft.EntityFrameworkCore;
//using System.Text.Encodings.Web;
//using System.Diagnostics;

//namespace TaskMan.Controllers
//{
//    public class UsersController : Controller
//    {
//        private readonly ApplicationDbContext _context;

//        public UsersController(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        // GET: Users
//        public async Task<IActionResult> Index()
//        {
//            ViewData["Header"] = "Users";
//            return View(await _context.User.ToListAsync());
//        }

//        // GET: User/Create
//        public IActionResult Create()
//        {
//            return View();
//        }

//        // POST: User/Create
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create(
//            [Bind("Username,Email,Password,Profile_Pic")] User user)
//        {
//            user.Creation_Date = DateOnly.FromDateTime(DateTime.Now);
//            user.Profile_Pic ??= "user.png";
//            if (ModelState.IsValid)
//            {
//                _context.User.Add(user);
//                await _context.SaveChangesAsync();
//                return RedirectToAction(nameof(Index));
//            }
//            if (!ModelState.IsValid)
//            {
//                // Log errors
//                var errors = ModelState.Values.SelectMany(v => v.Errors);
//                foreach (var error in errors)
//                {
//                    Debug.WriteLine(error.ErrorMessage);
//                }
//            }
//            return View(user);
//        }

//        // GET: User/Edit/5
//        public async Task<IActionResult> Edit(int id)
//        {
//            var user = await _context.User.FindAsync(id);
//            if (user == null) return NotFound();

//            return View("EditProfile", user);

//        }

//        // POST: User/Edit/5
//        [HttpPost, ActionName("Edit")]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Edit(int? id)
//        {
//            if (id == null) return NotFound();

//            var existingUser = await _context.User.FirstOrDefaultAsync(u => u.User_ID == id);

//            if (await TryUpdateModelAsync<User>(existingUser, "", u => u.Username, u => u.Email, u => u.Profile_Pic))
//            {
//                try
//                {
//                    await _context.SaveChangesAsync();
//                    return RedirectToAction(nameof(Index));
//                }
//                catch (DbUpdateException ex)
//                {
//                    // Log the exception
//                    Debug.WriteLine(ex.Message);
//                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
//                }
//            }
//            return View("EditProfile", existingUser);
//        }

//        // GET: User/Details/5
//        public async Task<IActionResult> Details(int id)
//        {
//            var user = await _context.User.FirstOrDefaultAsync(u => u.User_ID == id);

//            if (user == null) return NotFound();

//            return View(user);
//        }

//        // GET: User/Delete/5
//        public async Task<IActionResult> Delete(int id)
//        {
//            var user = await _context.User.FirstOrDefaultAsync(u => u.User_ID == id);

//            if (user == null) return NotFound();

//            return View(user);
//        }

//        // POST: User/Delete/5
//        [HttpPost, ActionName("Delete")]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> DeleteConfirmed(int id)
//        {
//            var user = await _context.User.FindAsync(id);

//            if (user == null)
//            {
//                return RedirectToAction(nameof(Index));
//            }

//            try
//            {
//                _context.User.Remove(user);
//                await _context.SaveChangesAsync();
//                return RedirectToAction(nameof(Index));
//            }
//            catch (DbUpdateException ex)
//            {
//                Debug.WriteLine(ex.Message);
//                return RedirectToAction(nameof(Delete));
//            }
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> AjaxDelete(int id)
//        {
//            var user = await _context.User.FindAsync(id);

//            if (user == null)
//            {
//                Debug.WriteLine($"User with ID {id} not found for deletion.");
//                return NotFound(new { success = false, message = "User not found." });
//            }
//            _context.User.Remove(user);
//            await _context.SaveChangesAsync();

//            return Json(new { success = true, message = $"{user.Username} deleted successfully." });
//        }
//    }
//}
