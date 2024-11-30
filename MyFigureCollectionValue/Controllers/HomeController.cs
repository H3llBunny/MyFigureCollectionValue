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
        private readonly ICurrencyConverterService _currencyConverterService;

        public HomeController(ILogger<HomeController> logger,
            IScraperService scraperService,
            IFigureService figureService,
            ICurrencyConverterService currencyConverterService)
        {
            _logger = logger;
            _scraperService = scraperService;
            _figureService = figureService;
            _currencyConverterService = currencyConverterService;
        }

        public async Task<IActionResult> Index(int pageNumber = 1, string sortOrder = "default")
        {
            if (!User.Identity.IsAuthenticated)
            {
                return View();
            }

            if (pageNumber <= 0)
            {
                return NotFound();
            }

            const int FiguresPerPage = 90;

            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var figures = await _figureService.GetAllFiguresAsync(userId, pageNumber, FiguresPerPage, sortOrder);

            if (figures.Any())
            {
                string userFigureCollectionUrl = await _figureService.GetUserFigureCollectionUrlAsync(userId);
                string figureCollectionUsername = userFigureCollectionUrl.Substring(userFigureCollectionUrl.IndexOf("/profile/") + 9);

                var figuresViewModel = new FiguresListViewModel
                {
                    FiguresPerPage = FiguresPerPage,
                    PageNumber = pageNumber,
                    SortOrder = sortOrder,
                    UserId = userId,
                    FiguresCount = await _figureService.GetUserFiguresCountAsync(userId),
                    Figures = figures,
                    UserFigureCollectionUrl = userFigureCollectionUrl,
                    FigureCollectionUsername = figureCollectionUsername,
                    SumRetailPriceCollection = await _figureService.SumRetailPriceCollectionAsync(userId),
                    SumAvgAftermarketPriceCollection = await _figureService.SumAvgAftermarketPriceCollectionAsync(userId)
                };

                return View(figuresViewModel);
            }

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
                return View();
            }

            if (string.IsNullOrWhiteSpace(profileUrl))
            {
                TempData["ErrorMessage"] = "Please ensure the URL is valid and try again";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                await _scraperService.LoginAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                TempData["ErrorMessage"] = "Service Unavailable (rush hour), please try again later.";
                return RedirectToAction(nameof(Index));
            }
            
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (_figureService.IsSameUserFigureCollection(userId, profileUrl))
            {
                TempData["ErrorMessage"] = "Same user collection, please provide a different url.";
                return RedirectToAction(nameof(Index));
            }

            List<string> links = new List<string>();

            try
            {
                links = (await _scraperService.GetAllFiguresLinkAsync(profileUrl)).ToList();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }

            await _figureService.RemoveUserFiguresAsync(userId);

            await _figureService.UpdateUserFigureCollectionUrlAsync(userId, profileUrl);

            var (newFigureList, retailPriceList, aftermarketPriceList) = await _scraperService.CreateFiguresAndPricesAsync(links, userId);

            if (newFigureList.Any())
            {
                await _figureService.AddFiguresAsync(newFigureList);
                await _figureService.AddUserFiguresAsync(userId, newFigureList);
            }

            if (retailPriceList.Any())
            {
                await _figureService.AddRetailPricesAsync(retailPriceList);
            }

            if (aftermarketPriceList.Count > 0)
            {
                await _figureService.AddAftermarketPricesAsync(aftermarketPriceList);

                var currentAftermarketPrices = aftermarketPriceList.Select(ap => new CurrentAftermarketPrice
                {
                    Id = ap.Id,
                    Price = ap.Price,
                    Currency = ap.Currency,
                    LoggedAt = ap.LoggedAt,
                    FigureId = ap.FigureId,
                });
                await _figureService.AddCurrentAftermarketPricesAsync(currentAftermarketPrices);
            }

            return RedirectToAction(nameof(Index));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
