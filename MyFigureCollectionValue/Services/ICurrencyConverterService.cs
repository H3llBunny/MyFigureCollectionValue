using MyFigureCollectionValue.Models;

namespace MyFigureCollectionValue.Services
{
    public interface ICurrencyConverterService
    {
        ICollection<RetailPrice> ConvertRetailPricesToUSD(ICollection<RetailPrice> retailPrices);

        ICollection<AftermarketPrice> ConvertAftermarketPricesToUSD(ICollection<AftermarketPrice> aftermarketPrices);
    }
}
