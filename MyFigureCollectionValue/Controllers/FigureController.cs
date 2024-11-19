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
            return this.View();
        }
    }
}
