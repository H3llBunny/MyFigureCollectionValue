﻿using MyFigureCollectionValue.Models;

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

        Task<IEnumerable<FigureInListViewModel>> GetAllFiguresAsync(string userId, int pageNumber, int figuresPerPage, string sortOrder);

        Task<string> GetUserFigureCollectionUrlAsync(string userId);

        Task<DateTime> GetUserFigureCollectionLastRefreshAsync(string userId);

        Task<decimal> SumRetailPriceCollectionAsync(string userId);

        Task<decimal> SumUserPurchasePriceAsync(string userId);

        Task<int> GetUserPurchasePriceCountAsync(string userId);

        Task<decimal> SumAvgAftermarketPriceCollectionAsync(string userId);

        Task<Dictionary<string, int>> GetFigureUrlsWithOutdatedAftermarketPricesAsync();

        Task UpdateFiguresLastUpdatedAftermarketPricesAsync(List<int> figureIds);

        Task<ICollection<string>> GetOutdatedFigureUrlsAsync();

        Task UpdateFiguresAsync(ICollection<Figure> figures);

        Task UpdateRetailPricesAsync(ICollection<RetailPrice> retailPrices);

        Task<FigurePageViewModel> GetFigureAsync(int figureId, string userId);

        bool IsSameUserFigureCollection(string userId, string profileUrl);

        Task AddPurchasePriceAsync(string userId, int figureId, decimal priceValue, string currency);

        bool IsCurrencySupported(string currency);
    }
}
