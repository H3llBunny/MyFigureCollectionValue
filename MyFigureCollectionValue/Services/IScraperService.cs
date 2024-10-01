namespace MyFigureCollectionValue.Services
{
    public interface IScraperService
    {
        Task<IEnumerable<string>> GetAllItemLinksAsync(string profileUrl);
    }
}
