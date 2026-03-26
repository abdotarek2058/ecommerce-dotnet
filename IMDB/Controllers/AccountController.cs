using IMDB.Data;
using IMDB.Data.Services;
using IMDB.Data.Static;
using IMDB.Data.ViewModel;
using IMDB.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace IMDB.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager , AppDbContext context, IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _emailService = emailService;
            

        }
        [Authorize(Roles = UserRoles.Admin)]
        public async Task <IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user =await _userManager.GetUserAsync(User);
            var model = new ProfileVM()
            {
                FullName = user.FullName
            };
            return View(model);
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileVM model) 
        { 
            var user = await _userManager.GetUserAsync(User);
            if(!ModelState.IsValid)
                return View(model);
            // تحديث الاسم
            user.FullName = model.FullName;

            // رفع الصورة
            if (model.ProfileImage != null)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/users");

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);
                // حذف الصورة القديمة
                if (!string.IsNullOrEmpty(user.ProfilePicture))
                {
                    var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfilePicture.TrimStart('/'));

                    if (System.IO.File.Exists(oldImagePath))
                        System.IO.File.Delete(oldImagePath);
                }

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ProfileImage.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProfileImage.CopyToAsync(stream);
                }

                user.ProfilePicture = "/images/users/" + fileName;
            }
            await _userManager.UpdateAsync(user);

            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                var result = await _userManager.ChangePasswordAsync(
                    user,
                    model.CurrentPassword,
                    model.NewPassword
                    );
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                        ModelState.AddModelError("", error.Description);
                    return View(model);
                }
            }
            ViewBag.Success = "Profile updated successfully";
            return View(model);
        }
        [Authorize(Roles = UserRoles.Admin)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

           

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction("Users");
            }
            return BadRequest();
        }
        [Authorize(Roles = UserRoles.Admin)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole(string id, string role)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            // منع تعديل الأدمن الرئيسي
            if (user.Email == "admin@imdb.com")
                return BadRequest("Cannot modify main admin");

            // إزالة كل الأدوار الحالية
            var roles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, roles);

            // إضافة الدور المختار
            var result = await _userManager.AddToRoleAsync(user, role);

            if (result.Succeeded)
                return RedirectToAction("Users");

            return BadRequest();
        }

        //public async Task<IActionResult> PromoteToAdmin(string id)
        //{
        //    var user = await _userManager.FindByIdAsync(id);
        //    if (user == null)
        //    {
        //        return NotFound();
        //    }
        //    var result = await _userManager.AddToRoleAsync(user, UserRoles.Admin);
        //    if (result.Succeeded)
        //    {
        //        return RedirectToAction("User");
        //    }
        //    return BadRequest();
        //}
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(Login model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.FindByNameAsync(model.UserName)?? await _userManager.FindByEmailAsync(model.UserName);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid login attempt");
                return View(model);
            }
            // 🔹 هنا نحط التحقق من تأكيد الإيميل
            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                ModelState.AddModelError("", "Please confirm your email first.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password,model.RememberMe, lockoutOnFailure: true);
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Movies");
            }
            ModelState.AddModelError(string.Empty, "Invalid Login Attempt");
            return View(model);
        }

        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Register model) { 
            if(!ModelState.IsValid)
            {
                return View(model);
            }
            var user = new ApplicationUser
            {
                UserName = model.FullName.Replace(" ",""),
                Email = model.Email,
                FullName = model.FullName
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                var confirmationLink = Url.Action(
                    "ConfirmEmail",
                    "Account",
                    new { userId = user.Id, token = token },
                    Request.Scheme);

                var templatePath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "EmailTemplates",
                    "ConfirmEmail.html"
                    );
                var template = System.IO.File.ReadAllText(templatePath);

                template = template.Replace("{{LINK}}", confirmationLink);

                await _emailService.SendEmailAsync(
                    user.Email,
                    "Confirm your IMDB account",
                    template
                );


                return View("RegisterConfirmation");
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);

        }

        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
                return RedirectToAction("Login");

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound();

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
                return View("EmailConfirmed");

            return View("Error");
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                return View("ForgotPasswordConfirmation");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var resetLink = Url.Action(
                "ResetPassword",
                "Account",
                new { token, email = model.Email },
                Request.Scheme);


            await _emailService.SendEmailAsync(
                model.Email,
                "Password Reset",
                $"You can reset your password by clicking this link: <a href='{resetLink}'>Reset Password</a>"
            );

            return View("ForgotPasswordConfirmation");
        }

        public IActionResult ResetPassword(string token, string email)
        {
            var model = new ResetPasswordVM
            {
                Token = token,
                Email = email
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                return RedirectToAction("Login");

            var result = await _userManager.ResetPasswordAsync(
                user,
                model.Token,
                model.Password);

            if (result.Succeeded)
                return RedirectToAction("Login");

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }
        //Aouth//

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            Console.WriteLine("ExternalLoginCallback CALLED");
            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                return RedirectToAction(nameof(Login));
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }
            // تسجيل الدخول باستخدام معلومات المزود الخارجي
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
            if (result.Succeeded)
            {
                return RedirectToLocal(returnUrl);
            }
            else
            {
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                var name = info.Principal.FindFirstValue(ClaimTypes.Name);

                if (string.IsNullOrEmpty(email))
                {
                    email = $"{info.ProviderKey}@githubuser.com";
                }

                // البحث عن مستخدم بالبريد
                var user = await _userManager.FindByEmailAsync(email);

                if (user == null)
                {
                    // إنشاء مستخدم جديد
                    user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        FullName = name
                    };

                    var createResult = await _userManager.CreateAsync(user);

                    if (!createResult.Succeeded)
                    {
                        foreach (var error in createResult.Errors)
                            ModelState.AddModelError("", error.Description);

                        return View("Login");
                    }
                }

                // ربط الحساب بالمزود الخارجي//
                var logins = await _userManager.GetLoginsAsync(user);
                if (!logins.Any(l => l.LoginProvider == info.LoginProvider && l.ProviderKey == info.ProviderKey))
                {
                    var addLoginResult = await _userManager.AddLoginAsync(user, info);

                    if (!addLoginResult.Succeeded)
                    {
                        foreach (var error in addLoginResult.Errors)
                            ModelState.AddModelError("", error.Description);

                        return View("Login");
                    }
                }
                var picture = info.Principal.FindFirstValue("picture");
                Console.WriteLine("PICTURE URL: " + picture);

                // لو كان تسجيل الدخول Facebook
                if (string.IsNullOrEmpty(picture) && info.LoginProvider == "Facebook")
                {
                    var facebookId = info.Principal.FindFirstValue(ClaimTypes.NameIdentifier);

                    if (!string.IsNullOrEmpty(facebookId))
                    {
                        picture = $"https://graph.facebook.com/{facebookId}/picture?type=large&return_ssl_resources=1";
                    }
                }


                if (!string.IsNullOrEmpty(picture))
                {
                    var claims = await _userManager.GetClaimsAsync(user);

                    if (!claims.Any(c => c.Type == "picture"))
                    {
                        await _userManager.AddClaimAsync(user, new Claim("picture", picture));
                    }
                }
                await _signInManager.SignOutAsync();
                await _signInManager.SignInAsync(user, isPersistent: false);

                return RedirectToLocal(returnUrl);
            }

        }


        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl)) 
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Movies");
            }
        }

    }
}
