
using Microsoft.EntityFrameworkCore;
using MyFigureCollectionValue.Data;

namespace MyFigureCollectionValue.Services
{
    public class LastUpdateService : ILastUpdateService
    {
        private readonly ApplicationDbContext _dbContext;

        public LastUpdateService(ApplicationDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public async Task<DateTime> GetLastDateForExchangeRateUpdateAsync()
        {
            return await this._dbContext.LastUpdate
                .Where(u => u.UpdateName == "CurrencyExchange")
                .Select(u => u.LastUpdated)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateLastExchangeRateDateAsync()
        {
            var lastUpdateEntity = await this._dbContext.LastUpdate
                .FirstOrDefaultAsync(u => u.UpdateName == "CurrencyExchange");


            lastUpdateEntity.LastUpdated = DateTime.UtcNow;
            await this._dbContext.SaveChangesAsync();
        }
    }
}
