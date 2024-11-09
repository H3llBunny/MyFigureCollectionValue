using Microsoft.AspNetCore.Mvc;
using MyFigureCollectionValue.Models;
using MyFigureCollectionValue.Services;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Security.Claims;

namespace MyFigureCollectionValue.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IScraperService _scraperService;
        private readonly IFigureService _figureService;
        private readonly ICurrencyConverterService _currencyConverterService;

        public HomeController(ILogger<HomeController> logger,
            IScraperService scraperService,
            IFigureService figureService,
            ICurrencyConverterService currencyConverterService)
        {
            _logger = logger;
            this._scraperService = scraperService;
            this._figureService = figureService;
            this._currencyConverterService = currencyConverterService;
        }

        public async Task<IActionResult> Index(int pageNumber = 1)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return View();
            }

            if (pageNumber <= 0)
            {
                return this.NotFound();
            }

            const int FiguresPerPage = 90;

            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var figures = await this._figureService.GetAllFiguresAsync(userId, pageNumber, FiguresPerPage);

            if (figures.Any())
            {
                string userFigureCollectionUrl = await this._figureService.GetUserFigureCollectionUrlAsync(userId);
                string figureCollectionUsername = userFigureCollectionUrl.Substring(userFigureCollectionUrl.IndexOf("/profile/") + 9);

                var figuresViewModel = new FiguresListViewModel
                {
                    FiguresPerPage = FiguresPerPage,
                    PageNumber = pageNumber,
                    UserId = userId,
                    FiguresCount = await this._figureService.GetUserFiguresCountAsync(userId),
                    Figures = figures,
                    UserFigureCollectionUrl = userFigureCollectionUrl,
                    FigureCollectionUsername = figureCollectionUsername,
                    SumRetailPriceCollection = await this._figureService.SumRetailPriceCollectionAsync(userId),
                    SumAvgAftermarketPriceCollection = await this._figureService.SumAvgAftermarketPriceCollectionAsync(userId)
                };

                return this.View(figuresViewModel);
            }

            return this.View();
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

            List<string> links = new List<string>();

            try
            {
                links = (await this._scraperService.GetAllFiguresLinkAsync(profileUrl)).ToList();
            }
            catch (Exception ex)
            {
                this.TempData["ErrorMessage"] = ex.Message;
                return this.RedirectToAction(nameof(this.Index));
            }

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await this._figureService.RemoveUserFiguresAsync(userId);

            await this._figureService.UpdateUserFigureCollectionUrlAsync(userId, profileUrl);

            var (newFigureList, retailPriceList, aftermarketPriceList) = await this._scraperService.CreateFiguresAndPricesAsync(links, userId);

            if (newFigureList.Count > 0)
            {
                await this._figureService.AddFiguresAsync(newFigureList);
                await this._figureService.AddUserFiguresAsync(userId, newFigureList);
            }

            if (retailPriceList.Count > 0)
            {
                var retailPricesInUSD = this._currencyConverterService.ConvertRetailPricesToUSD(retailPriceList);
                await this._figureService.AddRetailPricesAsync(retailPriceList);
            }

            if (aftermarketPriceList.Count > 0)
            {
                var aftermarketPricesInUSD = this._currencyConverterService.ConvertAftermarketPricesToUSD(aftermarketPriceList);

                await this._figureService.AddAftermarketPricesAsync(aftermarketPricesInUSD);

                var currentAftermarketPrices = aftermarketPricesInUSD.Select(ap => new CurrentAftermarketPrice
                {
                    Id = ap.Id,
                    Price = ap.Price,
                    Currency = ap.Currency,
                    LoggedAt = ap.LoggedAt,
                    FigureId = ap.FigureId,
                });
                await this._figureService.AddCurrentAftermarketPricesAsync(currentAftermarketPrices);
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
