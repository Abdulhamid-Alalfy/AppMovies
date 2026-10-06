using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Domin.Model;
using infrastructure.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace infrastructure.Services
{
    public interface IUserService
    {
        Task AddToFavoriteAsync(int movieId, AppUser user);
        Task<List<UserFavorite>> GetFavoritesAsync(AppUser user);
        Task RemoveFromFavoriteAsync(int favoriteId);

        Task<SignInResult> LoginAsync(LoginViewModel model);
        Task<IdentityResult> RegisterAsync(UserRegisterViewModel model );
        Task LogoutAsync();
        Task<UserProfileViewModel?> GetUserDetailsAsync(ClaimsPrincipal userPrincipal);
        Task<IdentityResult> ChangePasswordAsync(ChangePasswordViewModel model, ClaimsPrincipal principal);
        Task<EditUserProfileViewModel?> GetUserForEditAsync(string userId);
        Task<IdentityResult> UpdateUserAsync(EditUserProfileViewModel model);
        Task<bool> IsUserBlockedAsync(string userId);
    }
}
