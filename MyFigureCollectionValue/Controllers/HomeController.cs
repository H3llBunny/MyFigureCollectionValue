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
        private readonly IFigureService _figureService;

        public HomeController(ILogger<HomeController> logger,
            IScraperService scraperService,
            IFigureService figureService)
        {
            _logger = logger;
            this._scraperService = scraperService;
            this._figureService = figureService;
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

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var (newFigureList, retailPriceList) = await this._scraperService.CreateFiguresAndRetailPricesAsync(links, userId);

            if (newFigureList.Count > 0)
            {
                await this._figureService.AddFiguresAsync(newFigureList);
                await this._figureService.AddUserFiguresAsync(userId, newFigureList);
            }

            if (retailPriceList.Count > 0)
            {
                await this._figureService.AddRetailPricesAsync(retailPriceList);
            }

            return this.RedirectToAction(nameof(this.Index));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
