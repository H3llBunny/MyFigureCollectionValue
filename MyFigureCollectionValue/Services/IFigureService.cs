using MyFigureCollectionValue.Models;

namespace MyFigureCollectionValue.Services
{
    public interface IFigureService
    {
        Task<bool> DoesFigureExistAsync(int id);

        Task AddFiguresAsync(IEnumerable<Figure> figures);

        Task AddRetailPrices(IEnumerable<RetailPrice> retailPrices);

        Task AddUserFiguresAsync(string userId, IEnumerable<Figure> figureList);
    }
}
