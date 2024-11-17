using MyFigureCollectionValue.Models;

namespace MyFigureCollectionValue.Services
{
    public interface ICurrencyConverterService
    {
        Task<ICollection<RetailPrice>> ConvertRetailPricesToUSDAsync(ICollection<RetailPrice> retailPrices);

        Task<ICollection<AftermarketPrice>> ConvertAftermarketPricesToUSDAsync(ICollection<AftermarketPrice> aftermarketPrices);
    }
}
