using Domin.Model;
using infrastructure.Services;
using infrastructure.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
namespace MoviesApp.Areas.Admin.Controllers

{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AccountsController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly UserManager<AppUser> _userManager;


        public AccountsController(IAccountService accountService, UserManager<AppUser> userManager)
        {
            _accountService = accountService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _accountService.GetAllUsersAsync();
            var model = new AdminUsersIndexViewModel
            {
                Users = users,
                TotalUsers = users.Count,
                ActiveUsers = users.Count(u => !u.IsBlocked),
                AdminUsers = users.Count(u => u.Roles.Any(r => r.Contains("Admin", StringComparison.OrdinalIgnoreCase))),
                BlockedUsers = users.Count(u => u.IsBlocked)
            };

            return View(model);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Register()
        {
            var model = await _accountService.PrepareRegisterViewModelAsync();
            return View(model);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register(AdminRegisterUserViewModel model)
        {
            if (model.SelectedRoles != null)
            {
                model.SelectedRoles = model.SelectedRoles.Distinct().ToList();
            }

            if (ModelState.IsValid)
            {
                var result = await _accountService.RegisterUserAsync(model);
                if (result.Succeeded)
                    return RedirectToAction("Index");

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            if (model.SelectedRoles != null)
            {
                model.SelectedRoles = model.SelectedRoles.Distinct().ToList();
            }
            model.AvailableRoles = (await _accountService.PrepareRegisterViewModelAsync()).AvailableRoles;
            return View(model);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login() => View();
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _accountService.SignInUserAsync(model);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    await _accountService.SignOutAsync();
                    return RedirectToAction("Login");
                }

                // ✅ التحقق من الحظر فور نجاح تسجيل الدخول
                if (await _accountService.IsUserBlockedAsync(user.Id))
                {
                    await _accountService.SignOutAsync();
                    TempData["AccountLockedMessage"] = "تم إغلاق حسابك من قبل الإدارة. يرجى التواصل مع الدعم أو الإدارة لاستعادة الوصول.";
                    return RedirectToAction(nameof(AccountLocked));
                }

                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Contains("Admin"))
                {
                    return RedirectToAction("Index", "Accounts", new { area = "Admin" });
                }

                return RedirectToAction("Index", "Movie", new { area = "" });
            }

            if (result.IsLockedOut)
            {
                TempData["AccountLockedMessage"] = "تم تعليق حسابك مؤقتاً بسبب كثرة المحاولات الفاشلة. يرجى الانتظار 5 دقائق.";
                return RedirectToAction(nameof(AccountLocked));
            }

            ModelState.AddModelError(string.Empty, "The login details are incorrect.");
            return View(model);
        }

        [AllowAnonymous]
        public IActionResult AccountLocked()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var model = await _accountService.GetUserForEditAsync(id);
            if (model == null)
                return RedirectToAction("notFound", "Home");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(AdminUpdateUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await _accountService.UpdateUserAsync(model);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }
            
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            
            return View(model);
        }

        [HttpGet]
        public IActionResult ChgPass() => View();

        [HttpPost]
        public async Task<IActionResult> ChgPass(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await _accountService.ChangePasswordAsync(model, User);
            if (result.Succeeded)
            {
                return RedirectToAction("Index");
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await _accountService.SignOutAsync();
            return RedirectToAction("Login");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var result = await _accountService.DeleteUserAsync(id);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }


            foreach (var error in result.Errors)
            {
                TempData["ErrorMessage"] = error.Description;
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("notFound", "Home");
            }

            var userDetails = await _accountService.GetUserDetailsAsync(id);
            if (userDetails == null)
            {
                return RedirectToAction("notFound", "Home");
            }

            return View(userDetails);
        }

        [AllowAnonymous]
        public IActionResult AccessDenied(string URL)
        {
            return View();
        }
        
        [HttpGet]
        public async Task<IActionResult> ResetPassword(string id)
        {
            var user = await _accountService.GetUserDetailsAsync(id);
            if (user == null)
                return RedirectToAction("notFound", "Home");
                
            var model = new ResetPasswordViewModel
            {
                UserId = user.UserId,
                Email = user.Email,
                FullName = user.FullName
            };
            
            return View(model);
        }
        
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
                
            var result = await _accountService.AdminResetPasswordAsync(model.UserId, model.NewPassword);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Password has been successfully reset.";
                return RedirectToAction("Details", new { id = model.UserId });
            }
            
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            
            return View(model);
        }

        public async Task<IActionResult> BlockUser(string id)
        {
            var result = await _accountService.BlockUSer(id);

            if (!result)
                return NotFound();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> UnBlockUser(string id)
        {
            var result = await _accountService.UnBlockUSer(id);

            if (!result)
                return NotFound();

            return RedirectToAction(nameof(Index));
        }

    }
}
