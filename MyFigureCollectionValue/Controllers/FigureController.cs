using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyFigureCollectionValue.Services;
using System.Globalization;
using System.Security.Claims;

namespace MyFigureCollectionValue.Controllers
{
    public class FigureController : Controller
    {
        private readonly IFigureService _figureService;

        public FigureController(IFigureService figureService)
        {
            this._figureService = figureService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetFigure(int figureId)
        {
            if (!await _figureService.DoesFigureExistAsync(figureId))
            {
                TempData["ErrorMessage"] = "Figure Id does not exist.";
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var figure = await _figureService.GetFigureAsync(figureId, userId);
            return this.View(figure);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddPurchasePrice(int figureId, string price, string currency)
        {
            if (!await _figureService.DoesFigureExistAsync(figureId))
            {
                TempData["ErrorMessage"] = "Figure does not exist.";
                return RedirectToAction("Index", "Home");
            }

            if (!_figureService.IsCurrencySupported(currency))
            {
                TempData["ErrorMessage"] = "Currency not supported.";
                return RedirectToAction(nameof(GetFigure), new { figureId });
            }

            price = price.Replace(" ", "");

            CultureInfo cultureInfo = currency switch
            {
                "$" => new CultureInfo("en-US"), 
                "€" => new CultureInfo("fr-FR"), 
                "A$" => new CultureInfo("en-AU"),
                "C$" => new CultureInfo("en-CA"),
                "£" => new CultureInfo("en-GB"), 
                "HK$" => new CultureInfo("zh-HK"),
                "¥" => new CultureInfo("ja-JP"), 
            };

            if (!decimal.TryParse(price, NumberStyles.Any, cultureInfo, out decimal parsedPrice))
            {
                TempData["ErrorMessage"] = "Invalid price format. Please enter a valid number.";
                return RedirectToAction(nameof(GetFigure), new { figureId });
            }

            if (parsedPrice < 0)
            {
                TempData["ErrorMessage"] = "Price must be a positive number.";
                return RedirectToAction(nameof(GetFigure), new { figureId });
            }


            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                await _figureService.AddPurchasePriceAsync(userId, figureId, parsedPrice, currency);
                TempData["SuccessMessage"] = "Purchase price added successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to add purchase price: " + ex.Message;
            }


            return RedirectToAction(nameof(GetFigure), new { figureId });
        }
    }
}
