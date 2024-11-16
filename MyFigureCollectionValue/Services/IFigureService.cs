using MyFigureCollectionValue.Models;

namespace MyFigureCollectionValue.Services
{
    public interface IFigureService
    {
        Task<bool> DoesFigureExistAsync(int id);

        Task AddFiguresAsync(IEnumerable<Figure> figures);

        Task AddRetailPricesAsync(IEnumerable<RetailPrice> retailPrices);

        Task AddCurrentAftermarketPricesAsync(IEnumerable<CurrentAftermarketPrice> currentAftermarketPrices);

        Task AddAftermarketPricesAsync(IEnumerable<AftermarketPrice> aftermarketPrices);

        Task AddUserFiguresAsync(string userId, IEnumerable<Figure> figureList);

        Task RemoveUserFiguresAsync(string userId);

        Task AddExistingFigureToUserAsync(int figureId, string userId);

        Task<int> GetUserFiguresCountAsync(string userId);

        Task<IEnumerable<FigureInListViewModel>> GetAllFiguresAsync(string userId, int pageNumber, int figuresPerPage);

        Task UpdateUserFigureCollectionUrlAsync(string userId, string url);

        Task<string> GetUserFigureCollectionUrlAsync(string userId);

        Task<decimal> SumRetailPriceCollectionAsync(string userId);

        Task<decimal> SumAvgAftermarketPriceCollectionAsync(string userId);

        Task<Dictionary<string, int>> GetFigureUrlsWithOutdatedAftermarketPricesAsync();

        Task UpdateFiguresLastUpdatedRetailPricesAsync(List<int> figureIds);

        Task<List<string>> GetOutdatedFigureUrlsAsync();
    }
}
