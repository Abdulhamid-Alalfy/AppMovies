using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Domin.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using infrastructure.ViewModel;
using Microsoft.Extensions.Logging;
using infrastructure.Data;
namespace infrastructure.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ImageFileServices _imageFileServices;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<AccountService> _logger;
        private readonly DB _context;


        public AccountService(
        UserManager<AppUser> userManager,
        ImageFileServices imageFileServices,
        RoleManager<IdentityRole> roleManager,
        SignInManager<AppUser> signInManager,
        IWebHostEnvironment env,
        DB db,
        ILogger<AccountService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _imageFileServices = imageFileServices;
            _signInManager = signInManager;
            _env = env;
            _logger = logger;
            _context = db;
        }

        public async Task<List<UserRoleViewModel>> GetAllUsersAsync()
        {
            var data = await (
                from user in _context.Users
                join userRole in _context.UserRoles
                    on user.Id equals userRole.UserId into ur
                from userRole in ur.DefaultIfEmpty()

                join role in _context.Roles
                    on userRole.RoleId equals role.Id into roles
                from role in roles.DefaultIfEmpty()

                select new
                {
                    user.Id,
                    user.FullName,
                    user.Email,
                    user.ProfilePictureURL,
                    IsBlocked = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow,
                    RoleName = role != null ? role.Name : null
                }
            ).ToListAsync();

            var result = data
                .GroupBy(x => new
                {
                    x.Id,
                    x.FullName,
                    x.Email,
                    x.ProfilePictureURL,
                    x.IsBlocked
                })
                .Select(g => new UserRoleViewModel
                {
                    UserId = g.Key.Id,
                    FullName = g.Key.FullName,
                    Email = g.Key.Email,
                    ProfilePictureURL = g.Key.ProfilePictureURL,
                    IsBlocked = g.Key.IsBlocked,
                    Roles = g.Where(x => x.RoleName != null)
                             .Select(x => x.RoleName!)
                             .ToList()
                })
                .ToList();

            return result;
        }

        public Task<AdminRegisterUserViewModel> PrepareRegisterViewModelAsync()
        {
            var model = new AdminRegisterUserViewModel
            {
                AvailableRoles = _roleManager.Roles
                    .Where(r => !string.IsNullOrWhiteSpace(r.Name))
                    .Select(r => r.Name!)
                    .ToList()
            };
            return Task.FromResult(model);
        }

        public async Task<IdentityResult> RegisterUserAsync(AdminRegisterUserViewModel model)
        {
            string? imagePath = await _imageFileServices.UploadImageAsync(model.ImageFile);
            var user = new AppUser
            {
                FullName = model.FullName,
                Email = model.Email,
                UserName = model.Email,
                ProfilePictureURL = imagePath
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return result;
            }

            var selectedRoles = (model.SelectedRoles ?? new List<string>())
                .Where(role => !string.IsNullOrWhiteSpace(role))
                .Select(role => role.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (selectedRoles.Count == 0)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Description = "At least one role must be selected."
                });
            }

            var availableRoles = await _roleManager.Roles
                .Where(role => role.Name != null)
                .Select(role => role.Name!)
                .ToListAsync();

            var rolesToAssign = availableRoles
                .Where(role => selectedRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
                .ToList();

            if (rolesToAssign.Count != selectedRoles.Count)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Description = "One or more selected roles do not exist."
                });
            }

            var rolesResult = await _userManager.AddToRolesAsync(user, rolesToAssign);
            return rolesResult;
        }
        public async Task<SignInResult> SignInUserAsync(LoginViewModel model)
        {
            return await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);
        }


        public async Task<AdminUpdateUserViewModel?> GetUserForEditAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return null;


            var userRoles = await _userManager.GetRolesAsync(user);


            var rolesList = await _roleManager.Roles
            .Where(r => !string.IsNullOrWhiteSpace(r.Name))
            .Select(r => new SelectRoleViewModel
            {
                RoleName = r.Name!,
                IsSelected = userRoles.Contains(r.Name!)
            })
            .ToListAsync();


            var model = new AdminUpdateUserViewModel
            {
                UserId = user.Id,
                FullName = user.FullName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                PhotoPath = user.ProfilePictureURL,
                Roles = rolesList
            };


            return model;
        }

        public async Task<IdentityResult> UpdateUserAsync(AdminUpdateUserViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });

            // تحديث الصورة إذا وُجد ملف جديد
            if (model.ImageFile != null)
            {
                try
                {
                    if (!string.IsNullOrEmpty(user.ProfilePictureURL))
                    {
                        var oldPath = Path.Combine(
                            _env.WebRootPath ?? string.Empty,
                            user.ProfilePictureURL.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)
                        );

                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete old profile picture for user {UserId}", user.Id);
                    // لا نوقف التنفيذ عند فشل حذف الصورة القديمة — نكمل لرفع الصورة الجديدة
                }

                user.ProfilePictureURL = await _imageFileServices.UploadImageAsync(model.ImageFile);
            }

            // تحديث البيانات الأساسية (مع بعض التنظيف)
            user.FullName = model.FullName.Trim();

            // لو الايميل تغيّر -> نتأكد أنه مش مستخدم من قبل من قبل مستخدم آخر
            if (!string.Equals(user.Email, model.Email, StringComparison.OrdinalIgnoreCase))
            {
                var existing = await _userManager.FindByEmailAsync(model.Email);
                if (existing != null && existing.Id != user.Id)
                {
                    return IdentityResult.Failed(new IdentityError { Description = "Email is already taken by another user." });
                }

                user.Email = model.Email?.Trim();
                user.UserName = model.Email?.Trim();

                // optional: تحديث القيم المطبّعة
                user.NormalizedEmail = _userManager.NormalizeEmail(user.Email);
                user.NormalizedUserName = _userManager.NormalizeName(user.UserName);
            }

            // حفظ التغييرات الأساسية
            var userUpdate = await _userManager.UpdateAsync(user);
            if (!userUpdate.Succeeded)
                return userUpdate;

            // إدارة الأدوار: احسب الفرق بدل إزالة ثم إضافة كل شيء
            var currentRoles = (await _userManager.GetRolesAsync(user))
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .ToArray();

            var rolesToAdd = model.Roles?
                .Where(r => r.IsSelected && !string.IsNullOrWhiteSpace(r.RoleName))
                .Select(r => r.RoleName)
                .Distinct()
                .ToArray() ?? Array.Empty<string>();

            var rolesToRemove = currentRoles.Except(rolesToAdd).ToArray();
            var rolesActuallyToAdd = rolesToAdd.Except(currentRoles).ToArray();

            if (rolesToRemove.Length > 0)
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                if (!removeResult.Succeeded)
                    return removeResult;
            }

            if (rolesActuallyToAdd.Length > 0)
            {
                var addResult = await _userManager.AddToRolesAsync(user, rolesActuallyToAdd);
                if (!addResult.Succeeded)
                    return addResult;
            }

            // تجديد الكوكي/الـ principal للمستخدم الحالي (لو مطلوب)
            try
            {
                await _signInManager.RefreshSignInAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "RefreshSignInAsync failed for user {UserId}", user.Id);
                // لا نعيد الفشل لأن التحديث نفسه نجح — فقط نلوج المشكلة
            }

            return IdentityResult.Success;
        }
        public async Task<IdentityResult> ChangePasswordAsync(ChangePasswordViewModel model, ClaimsPrincipal principal)
        {
            var user = await _userManager.GetUserAsync(principal);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });
            }


            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
            }
            return result;
        }


        public async Task SignOutAsync()
        {
            await _signInManager.SignOutAsync();
        }


        public async Task<List<SelectListItem>> GetRolesAsync()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return roles.Select(r => new SelectListItem
            {
                Value = r.Name,
                Text = r.Name
            }).ToList();
        }

        public async Task<IdentityResult> DeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });
            }


            if (!string.IsNullOrEmpty(user.ProfilePictureURL) &&
            !user.ProfilePictureURL.Contains("defaultProfilePictureUrl.jpg", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var imagePath = Path.Combine(_env.WebRootPath ?? string.Empty, user.ProfilePictureURL.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (File.Exists(imagePath))
                    {
                        File.Delete(imagePath);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete profile picture for user {UserId}", user.Id);
                }
            }


            return await _userManager.DeleteAsync(user);
        }

        public async Task<AdminUserDetailsViewModel?> GetUserDetailsAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return null;


            var roles = await _userManager.GetRolesAsync(user);


            var userDetails = new AdminUserDetailsViewModel
            {
                UserId = user.Id,
                FullName = user.FullName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                ProfilePictureURL = user.ProfilePictureURL,
                Roles = roles.ToList()
            };


            return userDetails;
        }
        
        public async Task<IdentityResult> AdminResetPasswordAsync(string userId, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });
            }
            
            // Remove the old password first
            var removeResult = await _userManager.RemovePasswordAsync(user);
            if (!removeResult.Succeeded)
            {
                // If it fails because the user didn't have a password (e.g. social login), we can ignore and just Add
                if (removeResult.Errors.All(e => e.Code != "UserHasNoPassword"))
                    return removeResult;
            }
            
            // Add the new password
            var addResult = await _userManager.AddPasswordAsync(user, newPassword);
            return addResult;
        }

        public async Task<bool> BlockUSer(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;
            await _userManager.SetLockoutEnabledAsync(user,true);
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            return true;
        }

        public async Task<bool> UnBlockUSer(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;
            await _userManager.SetLockoutEndDateAsync(user, null);
            return true;
        }

        public async Task<bool> IsUserBlockedAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;
           return  await _userManager.IsLockedOutAsync(user);
        }


    }
}
