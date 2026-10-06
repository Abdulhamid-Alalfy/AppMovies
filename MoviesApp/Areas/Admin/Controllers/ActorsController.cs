using Domin.Model;
using infrastructure.Services;
using infrastructure.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static System.Net.Mime.MediaTypeNames;

namespace MoviesApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ActorsController : Controller
    {
        private readonly IActorService _actorService;

        public ActorsController(IActorService actorService)
        {
            _actorService = actorService;
        }

        public async Task<IActionResult> Index()
        {
            var actors = await _actorService.GetAllAsync();
            return View(actors);
        }

        public IActionResult Create() => View(new ActorVM());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ActorVM actorVM)
        {
            if (!ModelState.IsValid) return View(actorVM);

            var created = await _actorService.CreateAsync(actorVM);
            if (created == null)
            {
                ModelState.AddModelError("", "Failed to create actor.");
                return View(actorVM);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var actor = await _actorService.GetByIdAsync(id);
            if (actor == null) return NotFound();

            var vm = new EditVM
            {
                Id = actor.Id,
                Name = actor.Name,
                Bio = actor.Bio,
                Image = actor.Image
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditVM actorVM)
        {
            if (!ModelState.IsValid) return View(actorVM);

            var ok = await _actorService.UpdateAsync(actorVM);
            if (!ok) return NotFound();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _actorService.DeleteAsync(id);
            if (!ok) return NotFound();

            return RedirectToAction(nameof(Index));
        }
    }
}
