using MyFigureCollectionValue.Models;

namespace MyFigureCollectionValue.Services
{
    public interface IFigureService
    {
        Task<bool> DoesFigureExistAsync(int id);

        Task AddFiguresAsync(IEnumerable<Figure> figures);

        Task AddRetailPricesAsync(IEnumerable<RetailPrice> retailPrices);

        Task AddAftermarketPricesAsync(IEnumerable<AftermarketPrice> aftermarketPrices);

        Task AddUserFiguresAsync(string userId, IEnumerable<Figure> figureList);

        Task RemoveUserFiguresAsync(string userId);

        Task AddExistingFigureToUserAsync(int figureId, string userId);

        Task<int> GetUserFiguresCount(string userId);

        Task<IEnumerable<FigureInListViewModel>> GetAllFigures(string userId, int pageNumber, int figuresPerPage);
    }
}
