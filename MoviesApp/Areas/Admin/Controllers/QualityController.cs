using infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using static infrastructure.ViewModel.QualitMoviesVM;

namespace MoviesApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class QualityController : Controller
    {
        private readonly IQualityServices _quality;
        public QualityController(IQualityServices quality)
        {
            _quality = quality;
        }
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Movie");
        }

        [HttpGet]
        public async Task<IActionResult> AddQuality(int movieId)
        {
            var movie = await _quality.GetMovieDetailsByIdAsync(movieId);
            if (movie == null)
                return NotFound();

            var existing = await _quality.GetMovieQualitiesAsync(movieId);

            var model = new CreateMultipleMovieQualitiesViewModel
            {
                MovieId = movieId,
                MovieTitle = movie.Title,
                ExistingQualityTypes = existing.Select(q => q.Quality).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddQuality(CreateMultipleMovieQualitiesViewModel model)
        {
            var movie = await _quality.GetMovieDetailsByIdAsync(model.MovieId);
            model.MovieTitle = movie?.Title;
            model.ExistingQualityTypes = (await _quality.GetMovieQualitiesAsync(model.MovieId))
                .Select(q => q.Quality)
                .ToList();

            var validItems = (model.Items ?? new List<QualityUploadItem>())
                .Where(i => !string.IsNullOrWhiteSpace(i.QualityType) && i.VideoFile != null && i.VideoFile.Length > 0)
                .ToList();

            if (!validItems.Any())
            {
                ModelState.AddModelError(string.Empty, "اختر جودة واحدة على الأقل وارفع ملف فيديو مختلف لكل جودة");
                return View(model);
            }

            try
            {
                model.Items = validItems;
                var (added, skipped) = await _quality.AddMultipleMovieQualitiesAsync(model);

                if (added == 0 && skipped > 0)
                {
                    TempData["Error"] = "كل الجودات المختارة موجودة بالفعل لهذا الفيلم";
                    return View(model);
                }

                if (added > 0)
                {
                    TempData["Success"] = skipped > 0
                        ? $"تم إضافة {added} جودة، وتم تخطي {skipped} لأنها موجودة مسبقاً"
                        : $"تم إضافة {added} جودة بنجاح";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"فشل في إضافة الجودات: {ex.Message}";
                return View(model);
            }

            return RedirectToAction(nameof(ManageQualities), new { id = model.MovieId });
        }

        [HttpGet]
        public async Task<IActionResult> ManageQualities(int id)
        {
            var movie = await _quality.GetMovieDetailsByIdAsync(id);
            if (movie == null)
                return NotFound();

            var qualities = await _quality.GetMovieQualitiesAsync(id);

            var model = new ManageMovieQualitiesViewModel
            {
                MovieId = id,
                MovieTitle = movie.Title,
                MoviePoster = movie.Poster,
                ExistingQualities = qualities
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditQuality(int id)
        {
            var model = await _quality.GetQualityForEditAsync(id);
            if (model == null)
                return NotFound();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditQuality(EditMovieQualityViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // إعادة تحميل بيانات الفيلم في حالة الخطأ
                var quality = await _quality.GetQualityByIdAsync(model.Id);
                if (quality != null)
                {
                    var movie = await _quality.GetMovieDetailsByIdAsync(quality.MovieId);
                    model.MovieTitle = movie?.Title;
                }
                return View(model);
            }

            try
            {
                var result = await _quality.UpdateMovieQualityAsync(model);
                if (result)
                {
                    TempData["Success"] = "✅ تم تحديث الجودة بنجاح";
                }
                else
                {
                    TempData["Error"] = "❌ فشل في تحديث الجودة";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"❌ فشل في تحديث الجودة: {ex.Message}";
                return View(model);
            }

            return RedirectToAction(nameof(ManageQualities), new { id = model.MovieId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteQuality(int qualityId, int movieId)
        {
            try
            {
                var result = await _quality.DeleteMovieQualityAsync(qualityId);
                TempData[result ? "Success" : "Error"] = result
                    ? "🗑️ تم حذف الجودة بنجاح"
                    : "❌ فشل في حذف الجودة";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"❌ فشل في حذف الجودة: {ex.Message}";
            }

            return RedirectToAction(nameof(ManageQualities), new { id = movieId });
        }
    }
}
