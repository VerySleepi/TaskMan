using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TaskMan.Data;
using TaskMan.Models;
using TaskMan.Services;
using TaskMan.ViewModels.Account;

namespace TaskMan.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<User> signInManager;
        private readonly UserManager<User> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IEmailSender emailSender;
        private readonly ApplicationDbContext _context;

        public AccountController(SignInManager<User> signInManager, UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IEmailSender emailSender, ApplicationDbContext context)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.emailSender = emailSender;
            _context = context;
        }

        // GET: Login
        [HttpGet]
        // Attribute to prevent caching the login page so that the data is cleared when navigating to
        // the login page
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Login()
        {
            var model = new LoginViewModel();
            return View(model);
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var user = await userManager.FindByEmailAsync(model.Email);

                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }
                var result = await signInManager.PasswordSignInAsync(userName: user.UserName, password: model.Password, isPersistent: false, lockoutOnFailure: true);

                if (result.RequiresTwoFactor)
                {
                    return RedirectToAction("VerifyWith2fa", "Account", new { id = user.Id });
                }
                if (result.Succeeded)
                {
                    return RedirectToAction("Dashboard", "Tasks");
                }

                ModelState.AddModelError(string.Empty, "Invalid Login Attempt. Please try again.");
                return View(model);

            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                return View(model);
            }
        }

        // GET: Account/VerifyWith2fa
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> VerifyWith2fa(string id)
        {
            var twoFactorUser = await signInManager.GetTwoFactorAuthenticationUserAsync();
            if (twoFactorUser == null)
            {       
                return RedirectToAction("Login", "Account");
            }

            var user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                var model = new TwoFactorAuthenticationViewModel { UserId = id };
                var token = await userManager.GenerateTwoFactorTokenAsync(user, "Email");

                var senderEmail = "taskMan@taskman.com";
                var to = user.Email;
                var subject = "New Login Detected";
                var message = $"A new login to your account was detected at {DateTime.Now}. If this was you, please use the following code to complete the login process: {token}. If this wasn't you, we recommend changing your password immediately.";
                await emailSender.SendEmailAsync(senderEmail, to, subject, message);
                Console.WriteLine($"Sending email to {to} with token: {token}");

                return View(model);

            }

        }

        // POST: Account/VerifyWith2fa
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> VerifyWith2fa(TwoFactorAuthenticationViewModel model)
        {
            var user = await userManager.FindByIdAsync(model.UserId);
            //Debug.WriteLine($"Email: {email}, RememberMe: {rememberMe}, TwoFactorCode: {code}");
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var result = await signInManager.TwoFactorSignInAsync("Email", model.Code, isPersistent: false, rememberClient: false);
                if (result.Succeeded)
                {
                    await userManager.RemoveAuthenticationTokenAsync(user, "Email", "TwoFactorCode");
                    return RedirectToAction("Dashboard", "Tasks");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid code.");
                    return RedirectToAction("Login", "Account");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                return View(model);
            }

            
        }


        // GET: Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Initialise New User
            var user = new User
            {
                UserName = model.Username,
                NormalizedUserName = model.Username.ToUpper(),
                Email = model.Email,
                NormalizedEmail = model.Email.ToUpper(),
                // Needed to set this to true for now, as I haven't set up a way to send an actual verification email to the user,
                // but it would be set to false in a real application, and then set to true after the user verifies their email address.
                EmailConfirmed = true,
                Creation_Date = DateOnly.FromDateTime(DateTime.Now),
                // Due to poor time management, would have been able to set this in profile settings, set default to true for now
                TwoFactorEnabled = true
            };

            var result = await userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                var roleExist = await roleManager.RoleExistsAsync("User");

                // If the user role doesn't exist, create it
                if (!roleExist)
                {
                    var role = new IdentityRole("User");
                    await roleManager.CreateAsync(role);
                }

                await userManager.AddToRoleAsync(user, "User");

                return RedirectToAction("Login", "Account");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // GET: Account/VerifyEmail
        [HttpGet]
        public IActionResult VerifyEmail()
        {
            return View();

        }

        // POST: Account/VerifyEmail
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyEmail(VerifyEmailViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "User not found.");
                return View(model);
            }
            else
            {
                return RedirectToAction("ChangePassword", "Account", new { user.Email });
            }

        }

        // GET: Account/ChangePassword
        [HttpGet]
        public IActionResult ChangePassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("VerifyEmail", "Account");
            }

            return View(new ChangePasswordViewModel { Email = email });
        }

        //POST: Account/ChangePassword
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Something went wrong");
                return View(model);
            }

            var user = await userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "User not found!");
                return View(model);
            }

            var result = await userManager.RemovePasswordAsync(user);
            if (result.Succeeded)
            {
                result = await userManager.AddPasswordAsync(user, model.NewPassword);
                return RedirectToAction("Login", "Account");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(model);
            }
        }

        // POST: Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {        
            await signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}