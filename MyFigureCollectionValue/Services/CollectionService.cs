using Microsoft.EntityFrameworkCore;
using MyFigureCollectionValue.Data;
using MyFigureCollectionValue.Models;

namespace MyFigureCollectionValue.Services
{
    public class CollectionService : ICollectionService
    {
        private readonly ApplicationDbContext _dbContext;
        private const int CooldownTime = 15;

        public CollectionService(ApplicationDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public async Task DeleteUserFigureCollectionAsync(string usedId, string url)
        {
            var collectionUrl = await _dbContext.UserFigureCollectionUrls.FirstOrDefaultAsync(u => u.UserId == usedId && u.FigureCollectionUrl == url);

            if (collectionUrl != null)
            {
                _dbContext.UserFigureCollectionUrls.Remove(collectionUrl);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task UpdateUserFigureCollectionUrlAsync(string userId, string url)
        {
            var user = await _dbContext.UserFigureCollectionUrls.FirstOrDefaultAsync(u => u.UserId == userId);

            if (user != null)
            {
                user.FigureCollectionUrl = url;
                await _dbContext.SaveChangesAsync();
            }
            else
            {
                var userFigureCollectionUrl = new UserFigureCollectionUrl
                {
                    UserId = userId,
                    FigureCollectionUrl = url,
                    LastRefreshed = DateTime.UtcNow
                };

                await _dbContext.UserFigureCollectionUrls.AddAsync(userFigureCollectionUrl);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<UserFigureCollectionUrl> DoesCollectionUrlExist(string userId, string collectionUrl)
        {
            return await _dbContext.UserFigureCollectionUrls
                .FirstOrDefaultAsync(ufc => ufc.UserId == userId && ufc.FigureCollectionUrl == collectionUrl);
        }

        public bool CanRefresh(UserFigureCollectionUrl collection)
        {
            return collection.LastRefreshed.AddMinutes(CooldownTime) <= DateTime.UtcNow;
        }

        public async Task UpdateLastRefreshAsync(UserFigureCollectionUrl collection)
        {
            var userCollection = await _dbContext.UserFigureCollectionUrls.FindAsync(collection.UserId);

            userCollection.LastRefreshed = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
        }
    }
}
