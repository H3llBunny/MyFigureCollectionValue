using Microsoft.AspNetCore.Mvc;
using MyFigureCollectionValue.Models;
using MyFigureCollectionValue.Services;
using System.Diagnostics;
using System.Security.Claims;

namespace MyFigureCollectionValue.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IScraperService _scraperService;

        public HomeController(ILogger<HomeController> logger,
            IScraperService scraperService)
        {
            _logger = logger;
            this._scraperService = scraperService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddProfileUrl(string profileUrl)
        {
            if (string.IsNullOrWhiteSpace(profileUrl))
            {
                this.TempData["ErrorMessage"] = "Please ensure the URL is valid and try again";
                return this.RedirectToAction(nameof(this.Index));
            }

            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var links = this._scraperService.GetAllItemLinksAsync(profileUrl);
            
            return null;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
