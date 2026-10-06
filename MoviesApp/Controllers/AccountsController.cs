using Domin.Model;
using infrastructure.Services;
using infrastructure.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MoviesApp.Controllers
{
    [Authorize]
    public class AccountsController : Controller
    {
        private readonly IUserService _userService;
        private readonly UserManager<AppUser> _userManager;

        public AccountsController(IUserService userService, UserManager<AppUser> userManager)
        {
            _userService = userService;
            _userManager = userManager;
        }

        [AllowAnonymous]
        public IActionResult Login() => View();

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user?.LockoutEnd == DateTimeOffset.MaxValue)
            {
                return RedirectToAction(nameof(AccountLocked));
            }

            var result = await _userService.LoginAsync(model);
            if (result.Succeeded)
            {
                user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null && await _userService.IsUserBlockedAsync(user.Id))
                {
                    await _userService.LogoutAsync();
                    return RedirectToAction(nameof(AccountLocked));
                }
                return RedirectToAction("Index", "Movie");
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError("", "تم تعليق حسابك مؤقتاً بسبب كثرة المحاولات الفاشلة. يرجى الانتظار 5 دقائق.");
                return View(model);
            }

            ModelState.AddModelError("", "Invalid Email or Password");
            return View(model);
        }

        [AllowAnonymous]
        public IActionResult AccountLocked() => View();

        [AllowAnonymous]
        public IActionResult Register() => View();

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(UserRegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _userService.RegisterAsync(model);
            if (result.Succeeded)
                return RedirectToAction("Index", "Movie");

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await _userService.LogoutAsync();
            return RedirectToAction("Login", "Accounts");
        }

        public async Task<IActionResult> Details()
        {
            var userDetails = await _userService.GetUserDetailsAsync(User);
            if (userDetails == null)
                return RedirectToAction("Login");

            return View(userDetails);
        }

        [HttpGet]
        public IActionResult ChgPass() => View();

        [HttpPost]
        public async Task<IActionResult> ChgPass(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _userService.ChangePasswordAsync(model, User);
            if (result.Succeeded)
                return RedirectToAction("Index", "Movie");

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        public async Task<IActionResult> UpdateUser(string? id)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(id) || (!User.IsInRole("Admin") && id != currentUserId))
            {
                id = currentUserId;
            }

            if (string.IsNullOrEmpty(id))
                return RedirectToAction("Login");

            var model = await _userService.GetUserForEditAsync(id);
            if (model == null)
                return NotFound();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUser(EditUserProfileViewModel model)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (!User.IsInRole("Admin") && model.UserId != currentUserId)
            {
                model.UserId = currentUserId!;
            }

            if (!ModelState.IsValid)
                return View(model);

            var result = await _userService.UpdateUserAsync(model);
            if (result.Succeeded)
                return RedirectToAction("Details");

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        // ⚠️ TEMPORARY ACTION - إضافة User الحالي للـ Admin Role
        // يجب حذف هذا الـ Action بعد الاستخدام لأسباب أمنية!
        [AllowAnonymous]
        public async Task<IActionResult> MakeMeAdmin()
        {
            // تحقق من تسجيل الدخول
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return Content("يجب تسجيل الدخول أولاً! <a href='/Accounts/Login'>تسجيل الدخول</a>", "text/html");
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Content("خطأ: لم يتم العثور على المستخدم الحالي", "text/html");
            }

            // تحقق إذا كان User عنده Admin role فعلاً
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
            if (isAdmin)
            {
                return Content($"✅ المستخدم {currentUser.Email} عنده Admin Role فعلاً!<br><a href='/Admin/Home/Index'>الذهاب إلى Dashboard</a>", "text/html");
            }

            // إضافة User للـ Admin Role
            var result = await _userManager.AddToRoleAsync(currentUser, "Admin");
            if (result.Succeeded)
            {
                // تسجيل خروج ودخول لتحديث الـ Claims
                await _userService.LogoutAsync();
                return Content($"✅ تم إضافة {currentUser.Email} للـ Admin Role بنجاح!<br>يرجى <a href='/Accounts/Login'>تسجيل الدخول مرة أخرى</a> لتفعيل الصلاحيات.", "text/html");
            }

            var errors = string.Join("<br>", result.Errors.Select(e => e.Description));
            return Content($"❌ فشلت عملية الإضافة:<br>{errors}", "text/html");
        }
    }
}
