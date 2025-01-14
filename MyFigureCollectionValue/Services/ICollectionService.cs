using MyFigureCollectionValue.Models;

namespace MyFigureCollectionValue.Services
{
    public interface ICollectionService
    {
        Task DeleteUserFigureCollectionAsync(string usedId, string url);

        Task UpdateUserFigureCollectionUrlAsync(string userId, string url);

        Task<UserFigureCollectionUrl> DoesCollectionUrlExist(string userId, string collectionUrl);

        bool CanRefresh(UserFigureCollectionUrl collection);

        Task UpdateLastRefreshAsync(UserFigureCollectionUrl collection);
    }
}
