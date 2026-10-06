using Domin.Model;
using infrastructure.Services;
using infrastructure.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;

namespace MoviesApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProducerController : Controller
    {
        private readonly IProducerService _producerService;

        public ProducerController(IProducerService producerService)
        {
            _producerService = producerService;
        }

        public async Task<IActionResult> Index()
        {
            var producers = await _producerService.GetAllAsync();
            return View(producers);
        }

        public IActionResult Create() => View(new ProducerVM());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProducerVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var created = await _producerService.CreateAsync(vm);
            if (!created)
            {
                ModelState.AddModelError("", "Failed to create producer.");
                return View(vm);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var producer = await _producerService.GetByIdAsync(id);
            if (producer == null) return NotFound();

            var vm = new EditProducerVM
            {
                Id = producer.Id,
                Name = producer.Name,
                Bio = producer.Bio,
                Image = producer.Image
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProducerVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var ok = await _producerService.UpdateAsync(vm);
            if (!ok) return NotFound();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _producerService.DeleteAsync(id);
            if (!ok)
            {
                TempData["ProducerError"] = "Cannot delete this producer. It may not exist or it may already be linked to movies.";
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
