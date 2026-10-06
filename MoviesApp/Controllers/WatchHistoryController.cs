using Domin.Model;
using infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MoviesApp.Controllers
{
    [Authorize(Roles = "Admin,User")]
    public class WatchHistoryController : Controller
    {
        private readonly IWatchHistoryService _watchHistoryService;
        private readonly UserManager<AppUser> _userManager;

        public WatchHistoryController(IWatchHistoryService watchHistoryService, UserManager<AppUser> userManager)
        {
            _watchHistoryService = watchHistoryService;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> AddToHistory(int movieId, int progress = 0)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            try
            {
                var entryId = await _watchHistoryService.AddToWatchHistoryWithIdAsync(movieId, user, progress);

                // Return JSON for AJAX calls
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, watchHistoryId = entryId });
                }
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, error = ex.Message });
                return NotFound(ex.Message);
            }

            return RedirectToAction("Index", "Movie");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProgressByMovie(int movieId, int progress)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            try
            {
                var ok = await _watchHistoryService.UpdateProgressByMovieAsync(movieId, user.Id, progress);
                if (!ok)
                    return Json(new { success = false, error = "Watch history entry not found" });

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        public async Task<IActionResult> MyWatchHistory()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var watchHistory = await _watchHistoryService.GetWatchHistoryAsync(user);
            return View(watchHistory);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromHistory(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            try
            {
                await _watchHistoryService.RemoveFromWatchHistoryForUserAsync(id, user.Id);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }

            return RedirectToAction("MyWatchHistory");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearHistory()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            try
            {
                await _watchHistoryService.ClearWatchHistoryAsync(user);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }

            return RedirectToAction("MyWatchHistory");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProgress(int id, int progress)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            try
            {
                await _watchHistoryService.UpdateWatchProgressForUserAsync(id, user.Id, progress);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }

            return RedirectToAction("MyWatchHistory");
        }
    }
}
