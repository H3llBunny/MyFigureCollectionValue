namespace MyFigureCollectionValue.Services
{
    public interface ILastUpdateService
    {
        Task<DateTime> AftermarketPriceLastUpdateAsync();
    }
}
