using Domin.Model;
using infrastructure.Services;
using infrastructure.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MoviesApp.Controllers
{
    [AllowAnonymous]
    public class MovieController : Controller
    {
        private readonly IMoviesService _moviesService;

        public MovieController(IMoviesService moviesService)
            => _moviesService = moviesService;

        [HttpGet]
        public async Task<IActionResult> Index(string? searchTerm)
        {
            IEnumerable<MovieViewModel> movies;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                movies = await _moviesService.SearchMoviesAsync(searchTerm);
                ViewBag.SearchTerm = searchTerm;
            }
            else
            {
                movies = await _moviesService.GetAllMoviesAsync();
            }

            return View(movies);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();
            var movie = await _moviesService.GetMovieDetailsByIdAsync(id.Value);
            if (movie == null)
                return NotFound();

            return View(movie);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllGenres()
        {
            var genres = await _moviesService.GetAllGenresAsync();
            return View(genres); // نمررها للـ View
        }
        public async Task<IActionResult> Genres()
        {
            var genres = await _moviesService.GetAllGenresWithCountAsync();
            return View("GetAllGenres", genres);
        }


    
        [HttpGet]
        public async Task<IActionResult> ByGenre(int genreId, int page = 1)
        {
            var viewModel = await _moviesService.GetMoviesByGenreWithPaginationAsync(genreId, page);

            if (viewModel == null)
            {
                TempData["Error"] = "هذا النوع غير موجود";
                return RedirectToAction(nameof(Genres));
            }

            return View(viewModel);
        }

        public async Task<IActionResult> LatestMovies()
        {
            var movies = await _moviesService.GetLatestMoviesAsync(7);
            return PartialView("_LatestMovies", movies);
        }
    }
}
