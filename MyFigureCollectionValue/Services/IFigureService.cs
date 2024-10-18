using MyFigureCollectionValue.Models;

namespace MyFigureCollectionValue.Services
{
    public interface IFigureService
    {
        Task<bool> DoesFigureExistAsync(int id);

        Task AddFiguresAsync(IEnumerable<Figure> figures);

        Task AddRetailPricesAsync(IEnumerable<RetailPrice> retailPrices);

        Task AddUserFiguresAsync(string userId, IEnumerable<Figure> figureList);

        Task RemoveUserFiguresAsync(string userId);

        Task AddExistingFigureToUserAsync(int figureId, string userId);
    }
}
