using MyFigureCollectionValue.Models;

namespace MyFigureCollectionValue.Services
{
    public interface IScraperService
    {
        Task LoginAsync();

        Task<IEnumerable<string>> GetAllFiguresLinkAsync(string profileUrl);

        Task<ICollection<Figure>> CreateFiguresAndRetailPricesAsync(IEnumerable<string> figureUrls);

        Task<ICollection<RetailPrice>> GetRetailPriceList(string url, int figureId);
    }
}
