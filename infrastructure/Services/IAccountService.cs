using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using infrastructure.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace infrastructure.Services
{
    public interface IAccountService
    {
        Task<List<UserRoleViewModel>> GetAllUsersAsync();
        Task<AdminRegisterUserViewModel> PrepareRegisterViewModelAsync();
        Task<IdentityResult> RegisterUserAsync(AdminRegisterUserViewModel model);
        Task<SignInResult> SignInUserAsync(LoginViewModel model);
        Task<AdminUpdateUserViewModel?> GetUserForEditAsync(string id);
        Task<IdentityResult> UpdateUserAsync(AdminUpdateUserViewModel model);
        Task<IdentityResult> ChangePasswordAsync(ChangePasswordViewModel model, ClaimsPrincipal principal);
        Task SignOutAsync();
        Task<AdminUserDetailsViewModel?> GetUserDetailsAsync(string userId);
        Task<List<SelectListItem>> GetRolesAsync();
        Task<IdentityResult> DeleteUserAsync(string userId);
        Task<IdentityResult> AdminResetPasswordAsync(string userId, string newPassword);

        Task<bool> BlockUSer(string userId);
        Task<bool> UnBlockUSer(string userId);
        Task<bool> IsUserBlockedAsync(string userId);

    }
}
