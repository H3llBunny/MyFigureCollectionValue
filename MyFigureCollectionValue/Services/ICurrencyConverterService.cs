using MyFigureCollectionValue.Models;

namespace MyFigureCollectionValue.Services
{
    public interface ICurrencyConverterService
    {
        Task<ICollection<RetailPrice>> ConvertRetailPricesToUSDAsync(ICollection<RetailPrice> retailPrices);

        ICollection<T> ConvertAftermarketAndUserPurchasePricesToUSD<T>(ICollection<T> aftermarketPrices) where T : class;
    }
}
