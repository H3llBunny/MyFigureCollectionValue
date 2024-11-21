using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyFigureCollectionValue.Services;

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

            var figure = await _figureService.GetFigureAsync(figureId);
            return this.View(figure);
        }
    }
}
