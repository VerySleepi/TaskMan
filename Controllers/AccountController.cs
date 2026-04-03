using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
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

        public AccountController(SignInManager<User> signInManager, UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IEmailSender emailSender)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.emailSender = emailSender;
        }

        // GET: Login
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
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


            var result = await signInManager.PasswordSignInAsync(userName: user.UserName, password: model.Password, isPersistent: model.RememberMe, lockoutOnFailure: true);

            Debug.WriteLine(result.ToString(), model.Email, model.Password, model.RememberMe);

            if (result.RequiresTwoFactor)
            {
                return RedirectToAction("VerifyWith2fa", "Account", new { id = user.Id });
            }
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }
            

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        // GET: Account/VerifyWith2fa
        [HttpGet]
        public async Task<IActionResult> VerifyWith2fa(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new TwoFactorAuthenticationViewModel { UserId = id };
            var token = await userManager.GenerateTwoFactorTokenAsync(user, "Email");

            var senderEmail = "taskMan@taskman.com";
            var to = user.Email;
            var subject = "New Login Detected";
            var message = $"A new login to your account was detected at {DateTime.Now}. If this was you, please use the following code to complete the login process: {token}. If this wasn't you, we recommend changing your password immediately.";
            await emailSender.SendEmailAsync(senderEmail, to, subject, message);

            return View(new TwoFactorAuthenticationViewModel { UserId = id });
        }

        // POST: Account/VerifyWith2fa
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyWith2fa(TwoFactorAuthenticationViewModel model)
        {

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
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid code.");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                return View(model);
            }

            
        }

        //[HttpGet]
        //public async Task<IActionResult> TestEmail(string userEmail)
        //{
        //    var senderEmail = "George.O.Stead@student.shu.ac.uk";
        //    var subject = "New Login Detected";
        //    var message = $"A new login to your account was detected at {DateTime.Now}. If this was you, you can safely ignore this email. If this wasn't you, we recommend changing your password immediately.";

        //    //var emailSender = new EmailSender();
        //    //await emailSender.SendEmailAsync(senderEmail, userEmail, subject, message);
        //    return Content("Email sent! Check your SMTP server.");
        //}

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
                // Needed to set this to true for now, as we don't have a way to send a verification email to the user,
                // but it would be set to false in a real application, and then set to true after the user verifies their email address.
                EmailConfirmed = true,
                Creation_Date = DateOnly.FromDateTime(DateTime.Now),
                // Will be set to a default image for now, but the user can change it in their profile settings
                TwoFactorEnabled = true
            };

            var result = await userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                var roleExist = await roleManager.RoleExistsAsync("User");

                if (!roleExist)
                {
                    var role = new IdentityRole("User");
                    await roleManager.CreateAsync(role);
                }

                await userManager.AddToRoleAsync(user, "User");

                //await signInManager.SignInAsync(user, isPersistent: false);
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
                // TODO: Implement a way to send an email to the user with a code
                // to verify who they are, and then allow them to change their password after verifying the code.
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