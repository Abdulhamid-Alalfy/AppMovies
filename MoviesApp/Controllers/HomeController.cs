using Microsoft.AspNetCore.Mvc;

namespace MoviesApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult AccessDenied()
        {
            return View();
        }

        public IActionResult notFound() 
        {
        
            return View("notFound");
        }
    }
}
