using MyFigureCollectionValue.Models;

namespace MyFigureCollectionValue.Services
{
    public interface ICurrencyConverterService
    {
        ICollection<RetailPrice> ConvertToUSD(ICollection<RetailPrice> retailPrices);
    }
}
