using MyFigureCollectionValue.Models;

namespace MyFigureCollectionValue.Services
{
    public interface IScraperService
    {
        Task LoginAsync();

        Task<IEnumerable<string>> GetAllFiguresLinkAsync(string profileUrl);

        Task<ICollection<Figure>> CreateFiguresListAsync(IEnumerable<string> figureUrls);
    }
}
