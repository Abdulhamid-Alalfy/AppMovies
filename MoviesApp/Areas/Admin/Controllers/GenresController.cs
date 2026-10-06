using infrastructure.Services;
using infrastructure.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MoviesApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class GenresController : Controller
    {
        private readonly IGenreServices _genreSvc;    

        public GenresController(IGenreServices genreSvc)
        {
            _genreSvc = genreSvc;
        }
        public async Task<IActionResult> Index()
        {
            var genres = await _genreSvc.GetAllAsync();
            return View(genres);
        }

        public IActionResult Create() => View(new CreateGenreVM());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateGenreVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var created = await _genreSvc.CreateGenreAsync(vm);
            if (created == null)
            {
                ModelState.AddModelError("", "Failed to create genre.");
                return View(vm);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var genre = await _genreSvc.GetByIdAsync(id);
            if (genre == null) return NotFound();

            var vm = new UpdateGenreVM
            {
                Id = genre.Id,
                Name = genre.Name,
                NameAr = genre.NameAr,
                Description = genre.Description,
                Icon = genre.Icon
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateGenreVM vm)
        {
            if (!ModelState.IsValid) return View(vm);
            var ok = await _genreSvc.UpdateGenreAsync(vm.Id, vm);
            if (!ok) return NotFound();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _genreSvc.DeleteGenreAsync(id);
            if (!ok)
            {
                TempData["GenreError"] = "Cannot delete this genre. It may be linked to movies or does not exist.";
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
