using MyFigureCollectionValue.Models;

namespace MyFigureCollectionValue.Services
{
    public interface IScraperService
    {
        Task LoginAsync();

        Task<IEnumerable<string>> GetAllFiguresLinkAsync(string profileUrl);

        Task<(ICollection<Figure> Figures, ICollection<RetailPrice> RetailPrices, ICollection<AftermarketPrice> AftermarketPrices)> 
            CreateFiguresAndPricesAsync(IEnumerable<string> figureUrls, string userId);

        Task<ICollection<RetailPrice>> GetRetailPriceListAsync(string url, int figureId);

        Task<ICollection<AftermarketPrice>> GetAftermarketPriceListAsync(string url, int figureId);
    }
}
