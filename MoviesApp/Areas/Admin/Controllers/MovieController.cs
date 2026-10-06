using System.Security.Claims;
using Domin.Model;
using infrastructure.Services;
using infrastructure.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using static infrastructure.ViewModel.QualitMoviesVM;

namespace MoviesApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class MovieController : Controller
    {
        private readonly IMoviesService _moviesService;
        private readonly IWebHostEnvironment _env;
        private readonly INotificationService _notificationService;

        public MovieController(IMoviesService moviesService, IWebHostEnvironment env, INotificationService notificationService)
        {
            _moviesService = moviesService;
            _env = env;
            _notificationService = notificationService;
        }

        // ✅ عرض كل الأفلام
        public async Task<IActionResult> Index()
        {
            var movies = await _moviesService.GetAllMoviesAsync();
            return View(movies);
        }

        // ✅ عرض تفاصيل فيلم معين
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var movie = await _moviesService.GetMovieDetailsByIdAsync(id);
            if (movie == null)
                return NotFound();

            return View(movie);
        }

        // ✅ عرض صفحة الإضافة
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new CreateMovieViewModel();
            await PopulateLists(model);
            return View(model);
        }

        // ✅ تنفيذ الإضافة
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateMovieViewModel model)
        {
            if (!model.Year.HasValue)
            {
                ModelState.AddModelError(nameof(model.Year), "سنة الإصدار مطلوبة");
            }

            if (!ModelState.IsValid)
            {
                await PopulateLists(model);
                return View(model);
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var created = await _moviesService.AddMovieAsync(model, userId);
                if (!created)
                {
                    ModelState.AddModelError("", "فشل في إضافة الفيلم");
                    await PopulateLists(model);
                    return View(model);
                }

                TempData["Success"] = "✅ تم إضافة الفيلم بنجاح";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"حدث خطأ أثناء الإضافة: {ex.Message}");
                await PopulateLists(model);
                return View(model);
            }
        }

        // ✅ عرض صفحة التعديل
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _moviesService.GetMovieForEditAsync(id);
            if (model == null)
                return NotFound();

            await PopulateLists(model);
            return View(model);
        }

        // ✅ تنفيذ التعديل
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateMovieModelView model)
        {
            if (!model.Year.HasValue)
            {
                ModelState.AddModelError(nameof(model.Year), "سنة الإصدار مطلوبة");
            }

            if (!ModelState.IsValid)
            {
                await PopulateLists(model);
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var success = await _moviesService.UpdateMovieAsync(model, userId);
            if (!success)
            {
                ModelState.AddModelError("", "فشل في تعديل الفيلم");
                await PopulateLists(model);
                return View(model);
            }

            TempData["Success"] = "✅ تم تعديل الفيلم بنجاح";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var deleted = await _moviesService.DeleteMovieAsync(id, userId);
            if (!deleted)
                return NotFound();

            TempData["Success"] = "🗑️ تم حذف الفيلم بنجاح";
            return RedirectToAction(nameof(Index));
        }

       
        //[HttpGet]
        //public async Task<IActionResult> Player(int id, string quality = null)
        //{
        //    var playerData = await _moviesService.GetMoviePlayerDataAsync(id, quality);
        //    if (playerData == null)
        //        return NotFound();

        //    return View(playerData);
        //}
       
 
        private async Task PopulateLists(BaseMovieViewModel model)
        {
            var producers = await _moviesService.GetAllProducersAsync();
            var actors = await _moviesService.GetAllActorsAsync();
            var genres = await _moviesService.GetAllGenresAsync();

            model.ProducersList = producers.Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name }).ToList();
            model.ActorsList = actors.Select(a => new SelectListItem { Value = a.Id.ToString(), Text = a.Name }).ToList();
            model.GenresList = genres.Select(g => new SelectListItem { Value = g.Id.ToString(), Text = g.Name }).ToList();
        }
    }
}
