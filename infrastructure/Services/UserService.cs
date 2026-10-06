using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Domin.Model;
using infrastructure.Data;
using infrastructure.ViewModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly DB _context;
        private readonly ImageFileServices _imageFileService;
        private readonly IWebHostEnvironment _env;

        public UserService(ImageFileServices imageFileServices,IWebHostEnvironment env, DB context, UserManager<AppUser> userManager, SignInManager<AppUser>  signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _imageFileService = imageFileServices;
            _env = env;
        }

        public async Task AddToFavoriteAsync(int movieId, AppUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var movie = await _context.Movies.FindAsync(movieId);
            if (movie == null)
                throw new Exception("Movie not found");
            var existingFavorite = await _context.UserFavorites
                .FirstOrDefaultAsync(f => f.MovieId == movieId && f.UserId == user.Id);

            if (existingFavorite == null)
            {
                var favorite = new UserFavorite
                {
                    MovieId = movieId,
                    UserId = user.Id
                };

                _context.UserFavorites.Add(favorite);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<UserFavorite>> GetFavoritesAsync(AppUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var favorites = await _context.UserFavorites
                .Where(f => f.UserId == user.Id)
                .Include(f => f.Movies)
                .ToListAsync();

            return favorites;
        }

        public async Task RemoveFromFavoriteAsync(int favoriteId)
        {
            var favorite = await _context.UserFavorites.FindAsync(favoriteId);
            if (favorite == null)
                throw new Exception("Favorite not found");

            _context.UserFavorites.Remove(favorite);
            await _context.SaveChangesAsync();
        }


        public async Task<SignInResult> LoginAsync(LoginViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return SignInResult.Failed;

            return await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, true);
        }

        public async Task<IdentityResult> RegisterAsync(UserRegisterViewModel model)
        {

            string? imagePath = await _imageFileService.UploadImageAsync(model.ImageFile);

            var user = new AppUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                ProfilePictureURL = imagePath
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return result;

            await _userManager.AddToRoleAsync(user, "User");
            await _signInManager.SignInAsync(user, isPersistent: true);
            return result;
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<UserProfileViewModel?> GetUserDetailsAsync(ClaimsPrincipal userPrincipal)
        {
            var user = await _userManager.GetUserAsync(userPrincipal);
            if (user == null) return null;

            return new UserProfileViewModel
            {
                UserId = user.Id,
                FullName = user.FullName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                ProfilePictureURL = user.ProfilePictureURL
            };
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

        public async Task<EditUserProfileViewModel?> GetUserForEditAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            return new EditUserProfileViewModel
            {
                UserId = user.Id,
                FullName = user.FullName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                UserName = user.UserName ?? string.Empty,
                PhotoPath = user.ProfilePictureURL
            };
        }

        public async Task<IdentityResult> UpdateUserAsync(EditUserProfileViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });

        
            if (model.ImageFile != null)
            {
                if (!string.IsNullOrEmpty(user.ProfilePictureURL))
                {
                    var oldPath = Path.Combine(_env.WebRootPath, user.ProfilePictureURL.TrimStart('/'));
                    if (File.Exists(oldPath))
                        File.Delete(oldPath);
                }

              
                var newPath = await _imageFileService.UploadImageAsync(model.ImageFile);
                user.ProfilePictureURL = newPath;
            }

      
            user.FullName = model.FullName;

            // ✅ استخدام UserManager methods لضمان تحديث NormalizedEmail و NormalizedUserName
            if (!string.Equals(user.Email, model.Email, StringComparison.OrdinalIgnoreCase))
            {
                await _userManager.SetEmailAsync(user, model.Email);
            }
            if (!string.Equals(user.UserName, model.UserName, StringComparison.OrdinalIgnoreCase))
            {
                await _userManager.SetUserNameAsync(user, model.UserName);
            }

            return await _userManager.UpdateAsync(user);
        }

        public async Task<bool> IsUserBlockedAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;
            return await _userManager.IsLockedOutAsync(user);
        }
    }

}
