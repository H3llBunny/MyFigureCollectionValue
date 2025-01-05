using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MyFigureCollectionValue.Hubs;
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
        private readonly IHubContext<ScraperProgressHub> _hubContext;

        public HomeController(ILogger<HomeController> logger,
            IScraperService scraperService,
            IFigureService figureService,
            ICurrencyConverterService currencyConverterService,
            IHubContext<ScraperProgressHub> hubContext)
        {
            _logger = logger;
            _scraperService = scraperService;
            _figureService = figureService;
            _currencyConverterService = currencyConverterService;
            _hubContext = hubContext;
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

                if (userFigureCollectionUrl == null)
                {
                    return View();
                }

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
                    SumAvgAftermarketPriceCollection = await _figureService.SumAvgAftermarketPriceCollectionAsync(userId),
                    TotalPaid = await _figureService.SumUserPurchasePriceAsync(userId)
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
        [Authorize]
        public async Task<IActionResult> AddProfileUrl(string profileUrl)
        {
            if (string.IsNullOrWhiteSpace(profileUrl))
            {
                TempData["ErrorMessage"] = "Please ensure the URL is valid and try again";
                return Ok();
            }
            
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (_figureService.IsSameUserFigureCollection(userId, profileUrl))
            {
                TempData["ErrorMessage"] = "Same user collection, please provide a different url.";
                return Ok();
            }

            try
            {
                await _scraperService.LoginAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                TempData["ErrorMessage"] = "Service Unavailable (rush hour), please try again later.";
                return Ok();
            }

            var links = new List<string>();

            try
            {
                links = (await _scraperService.GetAllFiguresLinkAsync(profileUrl)).ToList();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return Ok();
            }

            await _figureService.RemoveUserFiguresAsync(userId);

            await _figureService.UpdateUserFigureCollectionUrlAsync(userId, profileUrl);

            var (newFigureList, retailPriceList, aftermarketPriceList) = await _scraperService.CreateFiguresAndPricesAsync(links, userId,
                async (current, total, status) =>
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveProgress", current, total, status);
                });

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

            return Ok();
        }

        [Authorize]
        public async Task<IActionResult> RemoveCollection(string collectionUrl)
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await _figureService.RemoveUserFiguresAsync(userId);
            await _figureService.DeleteUserFigureCollectionAsync(userId, collectionUrl);

            return RedirectToAction(nameof(Index));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
