using AngleSharp.Dom;
using MyFigureCollectionValue.Models;

namespace MyFigureCollectionValue.Services
{
    public interface IScraperService
    {
        Task LoginAsync();

        Task<IEnumerable<string>> GetAllFiguresLinkAsync(string profileUrl);

        Task<(ICollection<Figure> Figures, ICollection<RetailPrice> RetailPrices, ICollection<AftermarketPrice> AftermarketPrices)> 
            CreateFiguresAndPricesAsync(IEnumerable<string> figureUrls, string userId, Func<int, int, string, Task> progressCallback);

        Task<ICollection<RetailPrice>> GetRetailPriceListAsync(IDocument document, int figureId);

        Task<ICollection<AftermarketPrice>> GetAftermarketPriceListAsync(string url, int figureId, bool calledFromBackgroundService);

        Task<(ICollection<Figure> Figures, ICollection<RetailPrice> RetailPrices)> GetFiguresAndRetailPricesAsync(IEnumerable<string> figureUrls);
    }
}
