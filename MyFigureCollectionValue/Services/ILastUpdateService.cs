namespace MyFigureCollectionValue.Services
{
    public interface ILastUpdateService
    {
        Task<DateTime> GetLastDateForExchangeRateUpdateAsync();

        Task UpdateLastExchangeRateDateAsync();
    }
}
