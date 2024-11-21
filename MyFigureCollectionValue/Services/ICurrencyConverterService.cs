using MyFigureCollectionValue.Models;

namespace MyFigureCollectionValue.Services
{
    public interface ICurrencyConverterService
    {
        Task<ICollection<RetailPrice>> ConvertRetailPricesToUSDAsync(ICollection<RetailPrice> retailPrices);

        Task<ICollection<T>> ConvertAftermarketPricesToUSDAsync<T>(ICollection<T> aftermarketPrices) where T : class;
    }
}
