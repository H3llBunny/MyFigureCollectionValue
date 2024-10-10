using Microsoft.AspNetCore.Mvc;
using MyFigureCollectionValue.Models;
using MyFigureCollectionValue.Services;
using System.Diagnostics;

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
        public async Task<IActionResult> AddProfileUrl(string profileUrl)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return this.View();
            }

            if (string.IsNullOrWhiteSpace(profileUrl))
            {
                this.TempData["ErrorMessage"] = "Please ensure the URL is valid and try again";
                return this.RedirectToAction(nameof(this.Index));
            }

            await this._scraperService.LoginAsync();

            var links = (await this._scraperService.GetAllFiguresLinkAsync(profileUrl)).ToList();

            var figureList = await this._scraperService.CreateFiguresListAsync(links);

            return this.RedirectToAction(nameof(this.Index));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
