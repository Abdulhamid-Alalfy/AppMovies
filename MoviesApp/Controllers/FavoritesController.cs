using Domin.Model;
using infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace MoviesApp.Controllers
{
    [Authorize(Roles = "Admin,User")]
    public class FavoritesController : Controller
    {
        private readonly IUserService _favoritesService;
        private readonly UserManager<AppUser> _userManager;

        public FavoritesController(IUserService favoritesService, UserManager<AppUser> userManager)
        {
            _favoritesService = favoritesService;
            _userManager = userManager;
        }

        public async Task<IActionResult> AddToFavorite(int movieId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            try
            {
                await _favoritesService.AddToFavoriteAsync(movieId, user);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }

            return RedirectToAction("Index", "Movie");
        }

        [HttpPost]
        public async Task<IActionResult> Toggle(int movieId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "يجب تسجيل الدخول أولاً" });
            }

            try
            {
                var favorites = await _favoritesService.GetFavoritesAsync(user);
                var existing = favorites.FirstOrDefault(f => f.MovieId == movieId);

                if (existing != null)
                {
                    await _favoritesService.RemoveFromFavoriteAsync(existing.Id);
                    return Json(new { success = true, isFavorite = false, message = "تمت إزالة الفيلم من المفضلة" });
                }

                await _favoritesService.AddToFavoriteAsync(movieId, user);
                return Json(new { success = true, isFavorite = true, message = "تمت إضافة الفيلم للمفضلة" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> MyFavorites()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var favorites = await _favoritesService.GetFavoritesAsync(user);
            return View(favorites);
        }

        public async Task<IActionResult> RemoveFromFavorite(int favoriteId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            try
            {
                var favorites = await _favoritesService.GetFavoritesAsync(user);
                var existing = favorites.FirstOrDefault(f => f.Id == favoriteId);
                if (existing != null)
                {
                    await _favoritesService.RemoveFromFavoriteAsync(favoriteId);
                }
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }

            return RedirectToAction("MyFavorites");
        }
    }
}